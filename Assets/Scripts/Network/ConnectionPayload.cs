using System;

namespace Network
{
    [Serializable]
    public class ConnectionPayload
    {
        public string playerId;
        public string playerName;
        public bool isDebug;
        public string applicationVersion;
    }
}