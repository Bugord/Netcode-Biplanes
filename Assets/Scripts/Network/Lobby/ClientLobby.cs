using System;
using UI;
using UI.Screens;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace Network.Lobby
{
    public class ClientLobby : MonoBehaviour
    {
        [SerializeField]
        private NetcodeHooks netcodeHooks;

        [SerializeField]
        private NetworkLobby networkLobby;

        private LobbyScreen lobbyScreen;
        private NavigationSystem NavigationSystem => NavigationSystem.Instance;

        private void Awake()
        {
            netcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            netcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
        }

        private void OnDestroy()
        {
            netcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
            netcodeHooks.OnNetworkDespawnHook -= OnNetworkDespawn;
        }

        private void OnNetworkSpawn()
        {
            lobbyScreen = NavigationSystem.Push<LobbyScreen>();
            lobbyScreen.ReadyButtonPressed += OnReadyButtonPressed;
            
            networkLobby.LobbyPlayers.OnListChanged += OnLobbyPlayerStateChanged;
        }

        private void OnNetworkDespawn()
        {
            if (lobbyScreen) {
                lobbyScreen.ReadyButtonPressed -= OnReadyButtonPressed;
            }
            
            networkLobby.LobbyPlayers.OnListChanged -= OnLobbyPlayerStateChanged;
        }

        private void OnLobbyPlayerStateChanged(NetworkListEvent<LobbyPlayerState> changeEvent)
        {
            lobbyScreen.UpdatePlayersList(networkLobby.LobbyPlayers);    
        }

        private void OnReadyButtonPressed()
        {
            networkLobby.ToggleIsReadyRpc(netcodeHooks.NetworkManager.LocalClientId);
            Debug.Log($"[{nameof(ClientLobby)}] Ready button pressed");
        }
    }
}