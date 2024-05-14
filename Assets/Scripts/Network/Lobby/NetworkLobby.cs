﻿using System;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;

namespace Network.Lobby
{
    public class NetworkLobby : NetcodeHooks
    {
        public event Action<ulong> PlayerReadyPressed; 

        private NetworkList<LobbyPlayerState> lobbyPlayers;
        
        public NetworkList<LobbyPlayerState> LobbyPlayers => lobbyPlayers;

        private void Awake()
        {
            lobbyPlayers = new NetworkList<LobbyPlayerState>();
        }

        [Rpc(SendTo.Server)]
        public void ToggleIsReadyRpc(ulong clientId)
        {
            PlayerReadyPressed?.Invoke(clientId);
        }
    }
}