using Unity.Multiplayer.Samples.BossRoom;

namespace Network
{
    public struct SessionPlayerData : ISessionPlayerData
    {
        public readonly string PlayerName;
        
        public bool IsConnected { get; set; }
        public ulong ClientID { get; set; }

        public SessionPlayerData(ulong clientID, string playerName, bool isConnected = false)
        {
            ClientID = clientID;
            PlayerName = playerName;
            IsConnected = isConnected;
        }
        
        public void Reinitialize()
        {
        }
    }
}