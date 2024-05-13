using System;
using Unity.Netcode;
using UnityEngine;

namespace Pilot
{
    public class PilotParachuteController : NetworkBehaviour
    {
        public event Action ParachuteOpened;
        public event Action ParachuteHidden;

        [SerializeField]
        private Parachute parachute;

        private readonly NetworkVariable<int> parachuteDirection = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<bool> parachuteIsEnabled = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public bool WasParachuteOpened { get; private set; }

        private void Awake()
        {
            parachuteDirection.OnValueChanged += OnParachuteDirectionChanged;
            parachuteIsEnabled.OnValueChanged += OnParachuteIsEnabledChanged;
        }

        public override void OnDestroy()
        {
            parachuteDirection.OnValueChanged -= OnParachuteDirectionChanged;
            parachuteIsEnabled.OnValueChanged -= OnParachuteIsEnabledChanged;
        }

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
            SetParachuteIsEnabled(true);
        }

        public void HideParachute()
        {
            SetParachuteIsEnabled(false);
        }

        public void SetParachuteDirection(int direction)
        {
            parachuteDirection.Value = direction;
        }

        public void DestroyParachute()
        {
            DestroyParachuteRpc();
        }

        [Rpc(SendTo.Owner)]
        private void DestroyParachuteRpc()
        {
            HideParachute();
        }

        private void SetParachuteIsEnabled(bool isEnabled)
        {
            parachuteIsEnabled.Value = isEnabled;
            if (isEnabled) {
                ParachuteOpened?.Invoke();
            }
            else {
                ParachuteHidden?.Invoke();
            }
        }

        private void OnParachuteDirectionChanged(int previousDirection, int newDirection)
        {
            parachute.SetDirection(newDirection);
        }

        private void OnParachuteIsEnabledChanged(bool wasActive, bool isActive)
        {
            parachute.gameObject.SetActive(isActive);
        }
    }
}