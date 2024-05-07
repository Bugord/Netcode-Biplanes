using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Pilot
{
    public class PilotParachuteController : NetworkBehaviour
    {
        public event Action ParachuteOpened; 
        public event Action ParachuteHiden; 
        
        [SerializeField]
        private Parachute parachute;

        public bool WasParachuteOpened { get; private set; }

        public void Reset()
        {
            WasParachuteOpened = true;
        }

        public void OpenParachute()
        {
            if (WasParachuteOpened) {
                return;
            }

            WasParachuteOpened = true;
            ParachuteSetActiveRpc(true);
            ParachuteOpened?.Invoke();
        }

        public void HideParachute()
        {
            ParachuteSetActiveRpc(false);
        }

        public void SetParachuteDirection(int direction)
        {
            SetParachuteDirectionRpc(direction);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void ParachuteSetActiveRpc(bool isActive)
        {
            parachute.gameObject.SetActive(isActive);
            ParachuteHiden?.Invoke();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void SetParachuteDirectionRpc(int direction)
        {
            parachute.SetDirection(direction);
        }
    }
}