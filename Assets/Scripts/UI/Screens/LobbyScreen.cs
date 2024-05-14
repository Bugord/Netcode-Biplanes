using System;
using System.Collections.Generic;
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

        public void UpdatePlayersList(NetworkList<LobbyPlayerState> playersData)
        {
            for (var i = 0; i < playerSeats.Count; i++) {
                var playerSeat = playerSeats[i];
                if (i > playersData.Count) {
                    playerSeat.enabled = false;
                    break;
                }
                
                playerSeat.SetData(playersData[i]);
            }
        }
        
        public void OnReadyButtonPressed()
        {
            ReadyButtonPressed?.Invoke();
        }
    }
}