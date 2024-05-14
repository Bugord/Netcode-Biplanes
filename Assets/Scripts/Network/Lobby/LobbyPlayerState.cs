using System;
using Core;
using Unity.Collections;
using Unity.Netcode;

namespace Network.Lobby
{
    public struct LobbyPlayerState : INetworkSerializable, IEquatable<LobbyPlayerState>
    {
        public ulong ClientId;
        public FixedString32Bytes Name;
        public bool IsReady;
        public Team Team;

        public LobbyPlayerState(ulong clientId, string name, Team team)
        {
            ClientId = clientId;
            Name = new FixedString32Bytes(name);
            IsReady = false;
            Team = team;
        }

        public bool Equals(LobbyPlayerState other)
        {
            return Name.Equals(other.Name) && IsReady == other.IsReady;
        }

        public override bool Equals(object obj)
        {
            return obj is LobbyPlayerState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, IsReady);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref IsReady);
            serializer.SerializeValue(ref Team);
        }
    }
}