using UnityEngine;

namespace UI.Screens
{
    public class MainMenuScreen : BaseScreen
    {
        public void OnMultiplayerButtonPressed()
        {
            NavigationSystem.Instance.Replace<MultiplayerMenuScreen>();
        }

        public void OnExitButtonPressed()
        {
            Application.Quit();
        }
    }
}
