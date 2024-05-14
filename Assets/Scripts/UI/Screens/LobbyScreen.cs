using System;
using System.Collections.Generic;
using Network;
using Network.Lobby;
using Unity.Netcode;
using UnityEngine;

namespace UI.Screens
{
    public class LobbyScreen : BaseScreen
    {
        public event Action ReadyButtonPressed;

        [SerializeField]
        private List<PlayerSeat> playerSeats;
        
        private ConnectionManager ConnectionManager => ConnectionManager.Instance;

        private void Awake()
        {
            foreach (var playerSeat in playerSeats) {
                playerSeat.gameObject.SetActive(false);
            }
        }

        public void UpdatePlayersList(NetworkList<LobbyPlayerState> playersData)
        {
            for (var i = 0; i < playerSeats.Count; i++) {
                var playerSeat = playerSeats[i];
                if (i >= playersData.Count) {
                    playerSeat.gameObject.SetActive(false);
                    break;
                }
                playerSeat.gameObject.SetActive(true);
                playerSeat.SetData(playersData[i]);
            }
        }

        public void OnReadyButtonPressed()
        {
            ReadyButtonPressed?.Invoke();
        }

        public void OnBackButtonPressed()
        {
            ConnectionManager.RequestShutdown();
            NavigationSystem.Replace<MainMenuScreen>();
        }
    }
}