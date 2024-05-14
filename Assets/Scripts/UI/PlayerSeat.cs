using Network.Lobby;
using TMPro;
using UnityEngine;

namespace UI
{
    public class PlayerSeat : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI playerText;

        public void SetData(LobbyPlayerState playerState)
        {
            playerText.text =
                $"{playerState.Name}({playerState.ClientId}) - {playerState.Team} - {(playerState.IsReady ? "Ready" : "Not Ready")}";
        }
    }
}