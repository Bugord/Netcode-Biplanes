using System;
using Core;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using UnityEngine;

namespace Plane
{
    public class PlaneMovement : MonoBehaviour
    {
        public event Action EngineStarted;
        public event Action TookOff;

        [SerializeField]
        private Rigidbody2D rigidbody2D;

        [SerializeField]
        private NetworkedPlaneController networkedPlaneController;

        [SerializeField]
        private PlaneRotation planeRotation;

        [SerializeField]
        private ClientNetworkTransform clientNetworkTransform;

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

        private bool isInputDisabled;

        public bool WasEngineStarted { get; private set; }
        public bool DidTookOff { get; private set; }
       
        private float EdgeDistance => networkedPlaneController.EdgeDistance;

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
            if (!networkedPlaneController.IsSpawned) {
                return;
            }
            
            ChangeEngineEnabled();

            if (Mathf.Abs(transform.position.x) < EdgeDistance) {
                return;
            }

            MoveToOtherSide();
        }
        
        private void FixedUpdate()
        {
            if (isEngineOn) {
                ApplyForces();
            }
        }

        public void Teleport(Vector3 position)
        {
            clientNetworkTransform.Teleport(position, transform.rotation, transform.localScale);
        }

        public void Reset()
        {
            isEngineOn = false;
            WasEngineStarted = false;
            DidTookOff = false;
        }

        public void DisableInput()
        {
            isInputDisabled = true;
        }

        public void EnableInput()
        {
            isInputDisabled = false;
        }

        public void DisableEngine()
        {
            isEngineOn = false;
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
            if (isInputDisabled) {
                return;
            }
            
            if (Input.GetKey(KeyCode.W)) {
                isEngineOn = true;
                if (!WasEngineStarted) {
                    WasEngineStarted = true;
                    EngineStarted?.Invoke();
                }
            }
            if (Input.GetKey(KeyCode.S)) {
                isEngineOn = false;
            }
        }

        private void OnAngleChanged(float angle)
        {
            UpdateModifiers(angle);
            
            if (WasEngineStarted && !DidTookOff) {
                DidTookOff = true;
                TookOff?.Invoke();
            }
            isFacingRight = transform.right.x > 0;
        }

        private void UpdateModifiers(float angle)
        {
            currentFlightModifier = planeFlightModifiersData.GetModifiersForAngle(angle);
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
        
        private void MoveToOtherSide()
        {
            var playerPos = transform.position;
            clientNetworkTransform.Teleport(
                new Vector3(playerPos.x > 0 ? -EdgeDistance : EdgeDistance, playerPos.y),
                transform.rotation,
                Vector3.one);
        }
    }
}