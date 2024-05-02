using UI;
using Unity.Netcode;
using UnityEngine;

namespace Network.State
{
    public class HostingState : OnlineState
    {
        private const int MaxConnectPayload = 1024;
        private SessionManager<SessionPlayerData> SessionManager => SessionManager<SessionPlayerData>.Instance;

        public HostingState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public override void Enter()
        {
            NavigationSystem.Instance.PopToRoot();
            NavigationSystem.Instance.Push<OnlineScreen>();
        }

        public override void Exit()
        {
            SessionManager.OnServerEnded();
        }

        public override void OnClientConnected(ulong clientId)
        {
            var playerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
            if (playerData == null) {
                Debug.LogError($"No player data associated with client {clientId}");
                var reason = JsonUtility.ToJson(ConnectStatus.GenericDisconnect);
                ConnectionManager.NetworkManager.DisconnectClient(clientId, reason);
            }

            if (ConnectionManager.NetworkManager.ConnectedClientsIds.Count == 2) {
                GameManager.Instance.StartSession();
            }
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            if (clientId == ConnectionManager.NetworkManager.LocalClientId) {
                return;
            }

            var playerId = SessionManager.GetPlayerId(clientId);
            if (playerId == null) {
                return;
            }

            var sessionData = SessionManager.GetPlayerData(playerId);
            if (sessionData.HasValue) {
                Debug.Log($"[{nameof(HostingState)}] Player {sessionData.Value.PlayerName} disconnected");
            }

            SessionManager.DisconnectClient(clientId);
        }

        public override void OnUserRequestedShutdown()
        {
            var reason = JsonUtility.ToJson(ConnectStatus.HostEndedSession);
            for (var i = ConnectionManager.NetworkManager.ConnectedClientsIds.Count - 1; i >= 0; i--) {
                var id = ConnectionManager.NetworkManager.ConnectedClientsIds[i];
                if (id != ConnectionManager.NetworkManager.LocalClientId) {
                    ConnectionManager.NetworkManager.DisconnectClient(id, reason);
                }
            }
            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }

        public override void OnServerStopped()
        {
            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;
            if (connectionData.Length > MaxConnectPayload) {
                // If connectionData too high, deny immediately to avoid wasting time on the server. This is intended as
                // a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
                response.Approved = false;
                return;
            }

            var payload = System.Text.Encoding.UTF8.GetString(connectionData);

            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

            var gameReturnStatus = GetConnectStatus(connectionPayload);

            if (gameReturnStatus == ConnectStatus.Success) {
                SessionManager.SetupConnectingPlayerSessionData(clientId, connectionPayload.playerId,
                    new SessionPlayerData(clientId, connectionPayload.playerName, true));

                // connection approval will create a player object for you
                response.Approved = true;
                return;
            }

            response.Approved = false;
            response.Reason = JsonUtility.ToJson(gameReturnStatus);
        }

        private ConnectStatus GetConnectStatus(ConnectionPayload connectionPayload)
        {
            if (ConnectionManager.NetworkManager.ConnectedClientsIds.Count >= ConnectionManager.MaxConnectedPlayers) {
                return ConnectStatus.ServerFull;
            }

            if (connectionPayload.isDebug != Debug.isDebugBuild) {
                return ConnectStatus.IncompatibleBuildType;
            }

            return SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(connectionPayload.playerId)
                ? ConnectStatus.LoggedInAgain
                : ConnectStatus.Success;
        }
    }
}