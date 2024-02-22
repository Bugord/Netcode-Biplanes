using UI;

namespace Network.State
{
    public class OfflineState : BaseConnectionState
    {
        public NavigationSystem NavigationSystem => NavigationSystem.Instance;

        public OfflineState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public override void Enter()
        {
            ConnectionManager.NetworkManager.Shutdown();
            NavigationSystem.PopToRoot();
            NavigationSystem.Push<MainMenuScreen>();
        }

        public override void Exit()
        {
        }

        public override void StartClientIP(string playerName, string ipaddress, int port)
        {
            var connectionMethod = new ConnectionMethodIP(ipaddress, (ushort)port, ConnectionManager, playerName);
            ConnectionManager.ClientReconnecting.Configure(connectionMethod);
            ConnectionManager.ChangeState(ConnectionManager.ClientConnecting.Configure(connectionMethod));
        }

        public override void StartHostIP(string playerName, string ipaddress, int port)
        {
            var connectionMethod = new ConnectionMethodIP(ipaddress, (ushort)port, ConnectionManager, playerName);
            ConnectionManager.ChangeState(ConnectionManager.StartingHost.Configure(connectionMethod));
        }
    }
}