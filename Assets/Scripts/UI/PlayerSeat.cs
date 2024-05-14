using System;
using Network.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerSeat : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI playerNameText;

        [SerializeField]
        private TextMeshProUGUI isReadyText;

        [SerializeField]
        private Button readyButton;

        public void SetData(LobbyPlayerState playerState)
        {
            playerNameText.text = playerState.Name.Value;
            isReadyText.text = playerState.IsReady ? "Ready" : "Not Ready";
        }
    }
}