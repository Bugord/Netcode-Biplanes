namespace Network
{
    public struct SessionPlayerData : ISessionPlayerData
    {
        public readonly string PlayerName;
        public readonly bool IsHost;

        public bool IsConnected { get; set; }
        public ulong ClientID { get; set; }

        public SessionPlayerData(ulong clientID, string playerName, bool isConnected = false, bool isHost = false)
        {
            ClientID = clientID;
            PlayerName = playerName;
            IsConnected = isConnected;
            IsHost = isHost;
        }
        
        public void Reinitialize()
        {
        }
    }
}