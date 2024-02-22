using System.Collections;
using UnityEngine;

namespace Network.State
{
    public class ClientReconnectingState : ClientConnectingState
    {
        private Coroutine reconnectCoroutine;
        private int NbAttempts;

        const float TimeBeforeFirstAttempt = 1;
        const float TimeBetweenAttempts = 5;

        public ClientReconnectingState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public override void Enter()
        {
            NbAttempts = 0;
            reconnectCoroutine = ConnectionManager.StartCoroutine(ReconnectCoroutine());
        }

        public override void Exit()
        {
            if (reconnectCoroutine != null) {
                ConnectionManager.StopCoroutine(reconnectCoroutine);
                reconnectCoroutine = null;
            }

            Debug.Log($"[{nameof(ClientReconnectingState)}] could not reconnect to server");
        }

        public override void OnClientConnected(ulong _)
        {
            ConnectionManager.ChangeState(ConnectionManager.ClientConnected);
        }

        public override void OnClientDisconnect(ulong _)
        {
            var disconnectReason = ConnectionManager.NetworkManager.DisconnectReason;
            if (NbAttempts < ConnectionManager.NbReconnectAttempts) {
                if (string.IsNullOrEmpty(disconnectReason)) {
                    reconnectCoroutine = ConnectionManager.StartCoroutine(ReconnectCoroutine());
                }
                else {
                    var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                    Debug.Log($"[{nameof(ClientReconnectingState)}] could not reconnect to server: {connectStatus}");

                    switch (connectStatus) {
                        case ConnectStatus.UserRequestedDisconnect:
                        case ConnectStatus.HostEndedSession:
                        case ConnectStatus.ServerFull:
                        case ConnectStatus.IncompatibleBuildType:
                            ConnectionManager.ChangeState(ConnectionManager.Offline);
                            break;
                        default:
                            reconnectCoroutine = ConnectionManager.StartCoroutine(ReconnectCoroutine());
                            break;
                    }
                }
            }
            else {
                if (string.IsNullOrEmpty(disconnectReason)) {
                    Debug.Log($"[{nameof(ClientReconnectingState)}] Max reconnect try reached, disconnected");
                }
                else {
                    var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                    Debug.Log($"[{nameof(ClientReconnectingState)}] Max reconnect try reached, {connectStatus}");
                }

                ConnectionManager.ChangeState(ConnectionManager.Offline);
            }
        }

        private IEnumerator ReconnectCoroutine()
        {
            // If not on first attempt, wait some time before trying again, so that if the issue causing the disconnect
            // is temporary, it has time to fix itself before we try again. Here we are using a simple fixed cooldown
            // but we could want to use exponential backoff instead, to wait a longer time between each failed attempt.
            // See https://en.wikipedia.org/wiki/Exponential_backoff
            if (NbAttempts > 0) {
                yield return new WaitForSeconds(TimeBetweenAttempts);
            }

            Debug.Log($"[{nameof(ClientReconnectingState)}] Lost connection to host, trying to reconnect...");

            ConnectionManager.NetworkManager.Shutdown();

            yield return
                new WaitWhile(() =>
                    ConnectionManager.NetworkManager
                        .ShutdownInProgress); // wait until NetworkManager completes shutting down
            Debug.Log(
                $"[{nameof(ClientReconnectingState)}] Reconnecting attempt {NbAttempts + 1}/{ConnectionManager.NbReconnectAttempts}...");

            // If first attempt, wait some time before attempting to reconnect to give time to services to update
            // (i.e. if in a Lobby and the host shuts down unexpectedly, this will give enough time for the lobby to be
            // properly deleted so that we don't reconnect to an empty lobby
            if (NbAttempts == 0) {
                yield return new WaitForSeconds(TimeBeforeFirstAttempt);
            }

            NbAttempts++;
            var reconnectingSetupTask = ConnectionMethod.SetupClientReconnectionAsync();
            yield return new WaitUntil(() => reconnectingSetupTask.IsCompleted);

            if (!reconnectingSetupTask.IsFaulted && reconnectingSetupTask.Result.success) {
                // If this fails, the OnClientDisconnect callback will be invoked by Netcode
                var connectingTask = ConnectClientAsync();
                yield return new WaitUntil(() => connectingTask.IsCompleted);
            }
            else {
                if (!reconnectingSetupTask.Result.shouldTryAgain) {
                    // setting number of attempts to max so no new attempts are made
                    NbAttempts = ConnectionManager.NbReconnectAttempts;
                }
                // Calling OnClientDisconnect to mark this attempt as failed and either start a new one or give up
                // and return to the Offline state
                OnClientDisconnect(0);
            }
        }
    }
}