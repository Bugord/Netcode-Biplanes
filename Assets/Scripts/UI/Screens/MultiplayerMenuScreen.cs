using System.Text.RegularExpressions;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Screens
{
    public class MultiplayerMenuScreen : BaseScreen
    {
        [SerializeField]
        private TMP_InputField usernameInput;

        [SerializeField]
        private TMP_InputField ipAddressInput;

        [SerializeField]
        private TMP_InputField portInput;

        private ConnectionManager ConnectionManager => ConnectionManager.Instance;

        public void OnBackButtonPressed()
        {
            NavigationSystem.Replace<MainMenuScreen>();
        }

        public void OnHostButtonPressed()
        {
            var ipAddress = Sanitize(ipAddressInput.text);
            var port = ushort.Parse(Sanitize(portInput.text));
            
            ConnectionManager.StartHostIp(usernameInput.text, ipAddress, port);
            ConnectionManager.NetworkManager.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
        }

        public void OnClientButtonPressed()
        {
            var ipAddress = Sanitize(ipAddressInput.text);
            var port = ushort.Parse(Sanitize(portInput.text));

            ConnectionManager.StartClientIp(usernameInput.text, ipAddress, port);
        }

        static string Sanitize(string dirtyString)
        {
            return Regex.Replace(dirtyString, "[^A-Za-z0-9.]", "");
        }
    }
}