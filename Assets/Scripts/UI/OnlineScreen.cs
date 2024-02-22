using Network;
using UI;

public class OnlineScreen : BaseScreen
{
    public void OnDisconnectButtonPressed()
    {
        ConnectionManager.Instance.RequestShutdown();
    }
}