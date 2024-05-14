using System;
using Core;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network.Lobby
{
    public class ServerLobby : MonoBehaviour
    {
        [SerializeField]
        private NetcodeHooks netcodeHooks;

        [SerializeField]
        private NetworkLobby networkLobby;

        private SessionManager<SessionPlayerData> SessionManager => SessionManager<SessionPlayerData>.Instance;

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
            netcodeHooks.NetworkManager.SceneManager.OnSceneEvent += OnLoadComplete;
            netcodeHooks.NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            networkLobby.PlayerReadyPressed += OnPlayerReadyPressed;
        }

        private void OnNetworkDespawn()
        {
            netcodeHooks.NetworkManager.SceneManager.OnSceneEvent -= OnLoadComplete;
            netcodeHooks.NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }

        private void OnLoadComplete(SceneEvent sceneEvent)
        {
            if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) {
                return;
            }

            AddPlayerToLobby(sceneEvent.ClientId);
        }

        private void AddPlayerToLobby(ulong clientId)
        {
            var playerSessionData = SessionManager.GetPlayerData(clientId);

            if (!playerSessionData.HasValue) {
                Debug.LogError($"[{nameof(ServerLobby)}] Player data for client id {clientId} not found!");
                return;
            }

            var playerData = playerSessionData.Value;

            var team = GetAwailableTeam();
            networkLobby.LobbyPlayers.Add(new LobbyPlayerState(clientId, playerData.PlayerName, team,
                netcodeHooks.NetworkManager.LocalClientId == clientId));
            Debug.Log($"[{nameof(ServerLobby)}] Player {playerData.PlayerName}({clientId}) joined the lobby");
        }

        private void OnClientDisconnectCallback(ulong clientId)
        {
            for (var i = 0; i < networkLobby.LobbyPlayers.Count; i++) {
                var lobbyPlayer = networkLobby.LobbyPlayers[i];
                if (lobbyPlayer.ClientId == clientId) {
                    Debug.Log($"[{nameof(ServerLobby)}] Player {lobbyPlayer.Name}({clientId}) leaved from the lobby");
                    networkLobby.LobbyPlayers.RemoveAt(i);
                    break;
                }
            }
        }

        private void OnPlayerReadyPressed(ulong clientId)
        {
            var playerIndex = FindLobbyPlayerIndexById(clientId);
            var playerData = networkLobby.LobbyPlayers[playerIndex];
            playerData.IsReady = !playerData.IsReady;

            networkLobby.LobbyPlayers[playerIndex] = playerData;

            var readyCount = 0;
            foreach (var lobbyPlayerData in networkLobby.LobbyPlayers) {
                if (lobbyPlayerData.IsReady) {
                    readyCount++;
                }
            }

            if (readyCount == 2) {
                networkLobby.CloseLobbyUIRpc();
                netcodeHooks.NetworkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
            }
        }

        private Team GetAwailableTeam()
        {
            return networkLobby.LobbyPlayers.Count == 0 ? Team.Blue : Team.Red;
        }

        private int FindLobbyPlayerIndexById(ulong clientId)
        {
            for (var i = 0; i < networkLobby.LobbyPlayers.Count; ++i) {
                if (networkLobby.LobbyPlayers[i].ClientId == clientId) {
                    return i;
                }
            }
            return -1;
        }
    }
}