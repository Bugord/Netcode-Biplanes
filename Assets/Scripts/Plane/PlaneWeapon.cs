using Unity.Netcode;
using UnityEngine;

namespace Plane
{
    public class PlaneWeapon : NetworkBehaviour
    {
        [SerializeField]
        private Bullet bulletPrefab;

        [SerializeField]
        private float fireCooldown;

        [SerializeField]
        private float bulletSpawnDistance = 1.3f;

        private float lastFireTime;

        private void Update()
        {
            ProcessFire();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) {
                enabled = false;
            }
        }

        private void ProcessFire()
        {
            if (lastFireTime + fireCooldown > Time.time) {
                return;
            }

            if (!Input.GetKeyDown(KeyCode.Space)) {
                return;
            }

            lastFireTime = Time.time;

            FireRpc(transform.position + transform.right * bulletSpawnDistance, transform.rotation);
        }

        [Rpc(SendTo.Server)]
        private void FireRpc(Vector3 position, Quaternion rotation)
        {
            var bullet = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab.gameObject).GetComponent<Bullet>();

            bullet.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);
            bullet.Init(position, rotation);
        }
    }
}