using UnityEngine;

namespace Network.State
{
    public abstract class OnlineState : BaseConnectionState
    {
        protected OnlineState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }
        
        public override void OnUserRequestedShutdown()
        {
            // This behaviour will be the same for every online state
            Debug.Log($"[{nameof(OnlineState)}] User requested disconnection");
            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }

        public override void OnTransportFailure()
        {
            // This behaviour will be the same for every online state
            ConnectionManager.ChangeState(ConnectionManager.Offline);
        }
    }
}