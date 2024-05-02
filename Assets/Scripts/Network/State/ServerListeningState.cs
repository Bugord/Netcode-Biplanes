using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network.State
{
    /// <summary>
    /// Connection state corresponding to a listening server. Handles incoming client connections. When shutting down or
    /// being timed out, transitions to the Offline state.
    /// </summary>
    public class ServerListeningState : OnlineState
    {
        // used in ApprovalCheck. This is intended as a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
        private const int MaxConnectPayload = 1024;
        private bool minPlayerConnected = false;
        
        private SessionManager<SessionPlayerData> SessionManager => SessionManager<SessionPlayerData>.Instance;

        public ServerListeningState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public override void Enter()
        {
            minPlayerConnected = false;
        }

        public override void Exit()
        {
            SessionManager.OnServerEnded();
        }

        public override void OnClientConnected(ulong clientId)
        {
            Debug.Log($"[{nameof(ServerListeningState)}] {clientId} connected to the server.");
            
            var playerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
            if (playerData == null) {
                Debug.LogError($"[{nameof(ServerListeningState)}] No player data associated with client {clientId}");
                var reason = JsonUtility.ToJson(ConnectStatus.GenericDisconnect);
                ConnectionManager.NetworkManager.DisconnectClient(clientId, reason);
            }

            if (!minPlayerConnected && ConnectionManager.NetworkManager.ConnectedClientsIds.Count == 2) {
                minPlayerConnected = true;
                ConnectionManager.NetworkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
            }
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            var playerId = SessionManager.GetPlayerId(clientId);
            if (playerId == null) {
                return;
            }

            var sessionData = SessionManager.GetPlayerData(playerId);
            if (sessionData.HasValue) {
                Debug.Log($"[{nameof(ServerListeningState)}] Player {sessionData.Value.PlayerName} disconnected");
            }

            SessionManager.DisconnectClient(clientId);
            
            if (ConnectionManager.NetworkManager.ConnectedClientsIds.Count == 1 &&
                ConnectionManager.NetworkManager.ConnectedClients.ContainsKey(clientId)) {
                // This callback is invoked by the last client disconnecting from the server
                // Here the networked session is shut down immediately, but if we wanted to allow reconnection, we could
                // include a delay in a coroutine that could get cancelled when a client reconnects
                Debug.Log($"[{nameof(ServerListeningState)}] All clients have disconnected from the server. Shutting down");
                ConnectionManager.ChangeState(ConnectionManager.Offline);
                Quit();
            }
        }

        public override void OnUserRequestedShutdown()
        {
            var reason = JsonUtility.ToJson(ConnectStatus.ServerEndedSession);
            for (var i = 0; i < ConnectionManager.NetworkManager.ConnectedClientsIds.Count; i++) {
                var id = ConnectionManager.NetworkManager.ConnectedClientsIds[i];

                ConnectionManager.NetworkManager.DisconnectClient(id, reason);
            }
            ConnectionManager.ChangeState(ConnectionManager.Offline);
            Quit();
        }

        public override void OnServerStopped()
        {
            ConnectionManager.ChangeState(ConnectionManager.Offline);
            Quit();
        }

        /// <summary>
        /// This logic plugs into the "ConnectionApprovalResponse" exposed by Netcode.NetworkManager. It is run every time a client connects to us.
        /// The complementary logic that runs when the client starts its connection can be found in ClientConnectingState.
        /// </summary>
        /// <remarks>
        /// Multiple things can be done here, some asynchronously. For example, it could authenticate your user against an auth service like UGS' auth service. It can
        /// also send custom messages to connecting users before they receive their connection result (this is useful to set status messages client side
        /// when connection is refused, for example).
        /// </remarks>
        /// <param name="request"> The initial request contains, among other things, binary data passed into StartClient. In our case, this is the client's GUID,
        /// which is a unique identifier for their install of the game that persists across app restarts.
        ///  <param name="response"> Our response to the approval process. In case of connection refusal with custom return message, we delay using the Pending field.
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
            var connectionPayload =
                JsonUtility.FromJson<ConnectionPayload>(payload);
            var gameReturnStatus = GetConnectStatus(connectionPayload);

            if (gameReturnStatus == ConnectStatus.Success) {
                SessionManager.SetupConnectingPlayerSessionData(clientId, connectionPayload.playerId,
                    new SessionPlayerData(clientId, connectionPayload.playerName, true));
                
                response.Approved = true;
                return;
            }

            response.Approved = false;
            response.Reason = JsonUtility.ToJson(gameReturnStatus);
        }

        private ConnectStatus GetConnectStatus(ConnectionPayload connectionPayload)
        {
            if (ConnectionManager.NetworkManager.ConnectedClientsIds.Count >= 2) {
                return ConnectStatus.ServerFull;
            }

            if (connectionPayload.applicationVersion != Application.version) {
                return ConnectStatus.IncompatibleVersions;
            }
            
            return SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(connectionPayload.playerId)
                ? ConnectStatus.LoggedInAgain
                : ConnectStatus.Success;

            return ConnectStatus.Success;
            //todo add support to deny connection if map or game version is different
        }
        
        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}