using System.Threading.Tasks;
using Network;
using Network.ConnectionMethods;
using Unity.Netcode.Transports.UTP;

/// <summary>
/// Simple IP connection setup with UTP
/// </summary>
class ConnectionMethodIP : BaseConnectionMethod
{
    private readonly string ipAddress;
    private readonly ushort port;

    private const string HostIpAddress = "0.0.0.0";

    public ConnectionMethodIP(string ip, ushort port, ConnectionManager connectionManager, string playerName)
        : base(connectionManager, playerName)
    {
        ipAddress = ip;
        this.port = port;
        ConnectionManager = connectionManager;
    }

    public override async Task SetupClientConnectionAsync()
    {
        SetConnectionPayload(GetPlayerId(), PlayerName);
        var utp = (UnityTransport)ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
        utp.SetConnectionData(ipAddress, port);
    }

    public override async Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync()
    {
        // Nothing to do here
        return (true, true);
    }

    public override async Task SetupHostConnectionAsync()
    {
        SetConnectionPayload(GetPlayerId(), PlayerName); // Need to set connection payload for host as well, as host is a client too
        var utp = (UnityTransport)ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
        utp.SetConnectionData(HostIpAddress, port);
    }
}