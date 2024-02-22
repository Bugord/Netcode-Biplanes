using Network.State;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class ConnectionManager : MonoBehaviour
    {
        private static ConnectionManager connectionManager;
        public static ConnectionManager Instance => connectionManager;
        
        [SerializeField]
        private int nbReconnectAttempts = 2;
        
        private BaseConnectionState currentState;

        public const int MaxConnectedPlayers = 2;

        public int NbReconnectAttempts => nbReconnectAttempts;
        public NetworkManager NetworkManager => NetworkManager.Singleton;

        public OfflineState Offline { get; private set; }
        public ClientConnectingState ClientConnecting { get; private set; }
        public ClientConnectedState ClientConnected { get; private set; }
        public ClientReconnectingState ClientReconnecting { get; private set; }
        public StartingHostState StartingHost { get; private set; }
        public HostingState Hosting { get; private set; }

        private void Awake()
        {
            connectionManager = this;
            
            Offline = new OfflineState(this);
            ClientConnecting = new ClientConnectingState(this);
            ClientConnected = new ClientConnectedState(this);
            ClientReconnecting = new ClientReconnectingState(this);
            StartingHost = new StartingHostState(this);
            Hosting = new HostingState(this);
        }

        private void Start()
        {
            currentState = Offline;

            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            NetworkManager.OnServerStarted += OnServerStarted;
            NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.OnTransportFailure += OnTransportFailure;
            NetworkManager.OnServerStopped += OnServerStopped;
        }

        private void OnDestroy()
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            NetworkManager.OnServerStarted -= OnServerStarted;
            NetworkManager.ConnectionApprovalCallback -= ApprovalCheck;
            NetworkManager.OnTransportFailure -= OnTransportFailure;
            NetworkManager.OnServerStopped -= OnServerStopped;
        }

        public void StartClientLobby(string playerName)
        {
            currentState.StartClientLobby(playerName);
        }

        public void StartClientIp(string playerName, string ipaddress, int port)
        {
            currentState.StartClientIP(playerName, ipaddress, port);
        }

        public void StartHostLobby(string playerName)
        {
            currentState.StartHostLobby(playerName);
        }

        public void StartHostIp(string playerName, string ipaddress, int port)
        {
            currentState.StartHostIP(playerName, ipaddress, port);
        }

        public void RequestShutdown()
        {
            currentState.OnUserRequestedShutdown();
        }

        internal void ChangeState(BaseConnectionState nextState)
        {
            Debug.Log(
                $"[{typeof(ConnectionManager)}]: Changed connection state from {currentState.GetType().Name} to {nextState.GetType().Name}");

            currentState?.Exit();
            currentState = nextState;
            currentState.Enter();
        }

        private void OnClientDisconnectCallback(ulong clientId)
        {
            currentState.OnClientDisconnect(clientId);
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            currentState.OnClientConnected(clientId);
        }

        private void OnServerStarted()
        {
            currentState.OnServerStarted();
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            currentState.ApprovalCheck(request, response);
        }

        private void OnTransportFailure()
        {
            currentState.OnTransportFailure();
        }

        private void OnServerStopped(bool _)
        {
            currentState.OnServerStopped();
        }
    }
}