using System.Collections;
using Pilot;
using Unity.Netcode;
using UnityEngine;

namespace Plane
{
    public class Bullet : NetworkBehaviour
    {
        [SerializeField]
        private Rigidbody2D rigidbody2D;

        [SerializeField]
        private float speed;

        private const float Lifetime = 5f;

        public void Init(Vector2 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            rigidbody2D.velocity = transform.right * speed;

            InitRpc(position, rotation);
            StartCoroutine(DestroyBulletWithDelay());
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void InitRpc(Vector2 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
            rigidbody2D.velocity = transform.right * speed;
        }

        private IEnumerator DestroyBulletWithDelay()
        {
            yield return new WaitForSeconds(Lifetime);
            DestroyBullet();
        }

        private void DestroyBullet()
        {
            if (!IsSpawned) {
                return;
            }

            NetworkObject.Despawn();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!IsServer) {
                return;
            }

            if (col.TryGetComponent<NetworkedPlaneController>(out var networkedPlaneController)) {
                if (OwnerClientId == networkedPlaneController.OwnerClientId) {
                    return;
                }
                if (networkedPlaneController.IsCrashed) {
                    return;
                }
            }

            if (col.TryGetComponent<Health>(out var health)) {
                health.TakeDamage();
            }

            if (col.TryGetComponent<Parachute>(out var parachute)) {
                parachute.DestroyParachute();
            }

            if (col.TryGetComponent<NetworkedPilotController>(out var networkedPilotController)) {
                networkedPilotController.KillPlayerWithReason(PlaneDiedReason.PilotShot);
            }
        
            NetworkObject.Despawn();
        }
    }
}