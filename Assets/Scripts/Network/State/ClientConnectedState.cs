using UI;
using UnityEngine;

namespace Network.State
{
    public class ClientConnectedState : OnlineState
    {
        public ClientConnectedState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public override void Enter()
        {
            NavigationSystem.Instance.PopToRoot();
            NavigationSystem.Instance.Push<OnlineScreen>();
        }

        public override void Exit()
        {
        }

        public override void OnClientDisconnect(ulong _)
        {
            var disconnectReason = ConnectionManager.NetworkManager.DisconnectReason;
            if (string.IsNullOrEmpty(disconnectReason)) {
                Debug.Log($"[{nameof(ClientConnectingState)}] Disconnected from server. Reconnecting...");
                ConnectionManager.ChangeState(ConnectionManager.ClientReconnecting);
            }
            else {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                Debug.Log($"[{nameof(ClientConnectingState)}] Disconnected from server: {connectStatus}");
                ConnectionManager.ChangeState(ConnectionManager.Offline);
            }
        }
    }
}