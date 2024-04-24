using System;
using System.Threading.Tasks;
using Network.ConnectionMethods;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Netcode;
using UnityEngine;

namespace Network.State
{
    public class StartingHostState : OnlineState
    {
        private BaseConnectionMethod connectionMethod;

        public StartingHostState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public StartingHostState Configure(BaseConnectionMethod baseConnectionMethod)
        {
            connectionMethod = baseConnectionMethod;
            return this;
        }

        public override void Enter()
        {
            StartHost();
        }

        public override void Exit()
        {
        }

        public override void OnServerStarted()
        {
            ConnectionManager.ChangeState(ConnectionManager.Hosting);
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;
            // This happens when starting as a host, before the end of the StartHost call. In that case, we simply approve ourselves.
            if (clientId == ConnectionManager.NetworkManager.LocalClientId) {
                var payload = System.Text.Encoding.UTF8.GetString(connectionData);
                var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

                SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId,
                    connectionPayload.playerId,
                    new SessionPlayerData(clientId, connectionPayload.playerName, true, true));

                // connection approval will create a player object for you
                response.Approved = true;
            }
        }

        public override void OnServerStopped()
        {
            StartHostFailed();
        }

        private async Task StartHost()
        {
            try {
                await connectionMethod.SetupHostConnectionAsync();

                // NGO's StartHost launches everything
                if (!ConnectionManager.NetworkManager.StartHost()) {
                    StartHostFailed();
                }
            }
            catch (Exception) {
                StartHostFailed();
                throw;
            }
        }

        private void StartHostFailed()
        {
            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }
    }
}