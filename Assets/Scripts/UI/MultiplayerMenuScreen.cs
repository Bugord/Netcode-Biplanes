using Network;
using TMPro;
using UnityEngine;

namespace UI
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
            if (NetworkService.Instance.StartHost(portInput.text)) {
                NavigationSystem.Instance.PopLast();
            }
        }
        
        public void OnClientButtonPressed()
        {
            if (NetworkService.Instance.StartClient(ipAddressInput.text, portInput.text)) {
                NavigationSystem.Instance.PopLast();
            }
        }
    }
}