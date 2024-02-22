using System;
using System.Threading.Tasks;
using Network.ConnectionMethods;
using UnityEngine;

namespace Network.State
{
    public class ClientConnectingState : OnlineState
    {
        protected BaseConnectionMethod ConnectionMethod;

        public ClientConnectingState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public ClientConnectingState Configure(BaseConnectionMethod baseConnectionMethod)
        {
            ConnectionMethod = baseConnectionMethod;
            return this;
        }

        public override void Enter()
        {
            ConnectClientAsync();
        }
        
        public override void Exit()
        {
        }
        
        public override void OnClientConnected(ulong _)
        {
            Debug.Log($"[{nameof(ClientConnectingState)}] Connected to server");
            ConnectionManager.ChangeState(ConnectionManager.ClientConnected);
        }

        public override void OnClientDisconnect(ulong _)
        {
            // client ID is for sure ours here
            StartingClientFailed();
        }
        
        private void StartingClientFailed()
        {
            var disconnectReason = ConnectionManager.NetworkManager.DisconnectReason;
            if (string.IsNullOrEmpty(disconnectReason)) {
                Debug.Log(
                    $"[{nameof(ClientConnectingState)}] Could not connect to server: {ConnectStatus.StartClientFailed.ToString()}");
            }
            else {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                Debug.Log($"[{nameof(ClientConnectingState)}] Could not connect to server: {connectStatus}");
            }
            
            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }

        protected async Task ConnectClientAsync()
        {
            try {
                // Setup NGO with current connection method
                await ConnectionMethod.SetupClientConnectionAsync();

                // NGO's StartClient launches everything
                if (!ConnectionManager.NetworkManager.StartClient()) {
                    throw new Exception("NetworkManager StartClient failed");
                }
            }
            catch (Exception e) {
                Debug.LogError("Error connecting client, see following exception");
                Debug.LogException(e);
                StartingClientFailed();
                throw;
            }
        }
    }
}