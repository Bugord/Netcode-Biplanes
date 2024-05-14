using Network;

namespace UI.Screens
{
    public class OnlineScreen : BaseScreen
    {
        public void OnDisconnectButtonPressed()
        {
            ConnectionManager.Instance.RequestShutdown();
        }
    }
}