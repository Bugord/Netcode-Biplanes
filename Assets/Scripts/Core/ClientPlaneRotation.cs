using Unity.Netcode;
using UnityEngine;

namespace Core
{
    public class ClientPlaneRotation : NetworkBehaviour
    {
        [SerializeField]
        private float rorationCooldown = 0.1f;

        private const int AngleCount = 16;

        private readonly NetworkVariable<int> currentAngleIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private float lastRotationTime;
        private float angleDelta;

        private void Awake()
        {
            angleDelta = 360f / AngleCount;
        }

        public override void OnNetworkSpawn()
        {
            currentAngleIndex.OnValueChanged += OnCurrentAngleIndexChange;
            
            UpdateRoration();
        }

        public override void OnNetworkDespawn()
        {
            currentAngleIndex.OnValueChanged -= OnCurrentAngleIndexChange;
        }

        private void Update()
        {
            if (!IsOwner) {
                return;
            }

            ProcessAngleChange();
        }

        private void ProcessAngleChange()
        {
            if (lastRotationTime + rorationCooldown > Time.time) {
                return;
            }

            lastRotationTime = Time.time;

            var indexChange = GetIndexChange();
            ChangeAngleIndex(indexChange);
        }

        private int GetIndexChange()
        {
            if (Input.GetKey(KeyCode.A)) {
                return -1;
            }

            if (Input.GetKey(KeyCode.D)) {
                return 1;
            }

            return 0;
        }

        private void ChangeAngleIndex(int indexDelta)
        {
            if (indexDelta == 0) {
                return;
            }

            var newAngleIndex = currentAngleIndex.Value + indexDelta;
            if (newAngleIndex < 0) {
                newAngleIndex += AngleCount;
            }

            if (newAngleIndex >= AngleCount) {
                newAngleIndex -= AngleCount;
            }

            currentAngleIndex.Value = newAngleIndex;
        }

        private void OnCurrentAngleIndexChange(int previous, int current)
        {
            UpdateRoration();
        }

        private void UpdateRoration()
        {
            var angle = GetEulerAngle();
            RotateToAngle(angle);
        }

        private float GetEulerAngle()
        {
            var angle = 0f - currentAngleIndex.Value * angleDelta;
            if (angle >= 360f) {
                angle -= 360f;
            }

            return angle;
        }

        private void RotateToAngle(float angle)
        {
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}