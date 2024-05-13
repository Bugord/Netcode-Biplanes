using System;
using Unity.Netcode;
using UnityEngine;

namespace Plane
{
    public class PlaneRotation : NetworkBehaviour
    {
        public event Action<float> AngleChanged;

        [SerializeField]
        private float rorationCooldown = 0.1f;

        private const int AngleCount = 16;

        private readonly NetworkVariable<int> currentAngleIndex = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private float lastRotationTime;
        private float angleDelta;

        private bool isMirrored;
        private bool isRotationEnabled;
        private bool isRotatingDownDisabled;

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

            if (!isRotationEnabled) {
                return;
            }

            ProcessAngleChange();
        }

        public void Init(bool isMirrored)
        {
            this.isMirrored = isMirrored;
        }

        public void Reset()
        {
            currentAngleIndex.Value = 0;
        }

        public void EnableRotationUp()
        {
            isRotationEnabled = true;
            isRotatingDownDisabled = true;
        }

        public void DisableRotation()
        {
            isRotationEnabled = false;
        }

        public void EnableRotationDown()
        {
            isRotatingDownDisabled = false;
        }

        private void ProcessAngleChange()
        {
            if (lastRotationTime + rorationCooldown > Time.time) {
                return;
            }

            lastRotationTime = Time.time;

            var indexChange = GetIndexChange();

            if (isMirrored) {
                indexChange *= -1;
            }

            if (isRotatingDownDisabled && indexChange == 1) {
                indexChange = 0;
            }

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

            AngleChanged?.Invoke(angle);
        }

        private float GetEulerAngle()
        {
            var angle = 360f - currentAngleIndex.Value * angleDelta;
            if (angle >= 360f) {
                angle -= 360f;
            }

            return angle;
        }

        private void RotateToAngle(float angle)
        {
            var cachedRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(cachedRotation.x, cachedRotation.y, angle);
        }
    }
}