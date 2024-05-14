using System.Text.RegularExpressions;
using Network;
using TMPro;
using UnityEngine;

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

        public void OnBackButtonPressed()
        {
            NavigationSystem.Instance.Replace<MainMenuScreen>();
        }

        public void OnHostButtonPressed()
        {
            var ipAddress = Sanitize(ipAddressInput.text);
            var port = ushort.Parse(Sanitize(portInput.text));

            ConnectionManager.Instance.StartHostIp(usernameInput.text, ipAddress, port);
        }

        public void OnClientButtonPressed()
        {
            var ipAddress = Sanitize(ipAddressInput.text);
            var port = ushort.Parse(Sanitize(portInput.text));

            ConnectionManager.Instance.StartClientIp(usernameInput.text, ipAddress, port);
        }

        static string Sanitize(string dirtyString)
        {
            return Regex.Replace(dirtyString, "[^A-Za-z0-9.]", "");
        }
    }
}