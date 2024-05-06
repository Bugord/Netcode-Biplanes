using System;
using UnityEngine;

namespace Core
{
    public class PlaneMovement : MonoBehaviour
    {
        public event Action TookOff;
        
        [SerializeField]
        private Rigidbody2D rigidbody2D;

        [SerializeField]
        private NetworkedPlaneController networkedPlaneController;

        [SerializeField]
        private PlaneRotation planeRotation;

        [SerializeField]
        private PlaneFlightModifiersData planeFlightModifiersData;

        [SerializeField]
        private float engineBaseForce;

        [SerializeField]
        private float liftBaseForce;

        [SerializeField]
        private float maxSpeed;

        private bool isEngineOn;
        private FlightModifier currentFlightModifier;

        private bool isFacingRight;

        private bool didTookOff;
        
        private void Awake()
        {
            networkedPlaneController.OnNetworkSpawnHook += OnNetworkSpawn;
            planeRotation.AngleChanged += OnAngleChanged;
        }

        private void OnDestroy()
        {
            networkedPlaneController.OnNetworkSpawnHook -= OnNetworkSpawn;
            planeRotation.AngleChanged -= OnAngleChanged;
        }

        private void OnNetworkSpawn()
        {
            if (!networkedPlaneController.IsOwner) {
                enabled = false;
            }
        }

        private void Update()
        {
            ChangeEngineEnabled();
        }

        private void FixedUpdate()
        {
            if (isEngineOn) {
                ApplyForces();
            }
        }

        public void DisableMovement()
        {
            rigidbody2D.velocity = Vector2.zero;
            rigidbody2D.isKinematic = true;
        }

        public void EnableMovement()
        {
            rigidbody2D.isKinematic = false;
        }

        private void ChangeEngineEnabled()
        {
            if (Input.GetKey(KeyCode.W)) {
                isEngineOn = true;
                if (!didTookOff) {
                    didTookOff = true;
                    TookOff?.Invoke();
                }
            }
            if (Input.GetKey(KeyCode.S)) {
                isEngineOn = false;
            }
        }

        private void OnAngleChanged(float angle)
        {
            UpdateModifiers(angle);
            isFacingRight = transform.right.x > 0;
        }

        private void UpdateModifiers(float angle)
        {
            currentFlightModifier = planeFlightModifiersData.GetModifiersForAngle(angle);
            Debug.Log($"New modifiers: {currentFlightModifier.engineModifier} {currentFlightModifier.liftModifier}");
        }

        private void ApplyForces()
        {
            var engineForce = engineBaseForce * currentFlightModifier.engineModifier;
            var liftForce = liftBaseForce * currentFlightModifier.liftModifier;

            if (!isFacingRight) {
                engineForce = new Vector2(-engineForce.x, engineForce.y);
            }
            
            rigidbody2D.AddForce(engineForce + liftForce);

            if (rigidbody2D.velocity.magnitude > maxSpeed) {
                rigidbody2D.velocity = rigidbody2D.velocity.normalized * maxSpeed;
            }
        }
    }
}