using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody2D rigidbody2D;

    [SerializeField]
    private Bullet bulletPrefab;

    [SerializeField]
    private float fireCooldown;

    private float lastFireTime;

    private void Update()
    {
        if (!IsOwner) {
            return;
        }
        
        CheckFire();
    }

    private void CheckFire()
    {
        if (lastFireTime + fireCooldown > Time.time) {
            return;
        }

        if (!Input.GetKeyDown(KeyCode.Space)) {
            return;
        }

        lastFireTime = Time.time;

        FireRpc();
    }

    [Rpc(SendTo.Server)]
    private void FireRpc()
    {
        var bullet = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab.gameObject).GetComponent<Bullet>();
        bullet.Config(transform.position + transform.right, transform.rotation);
        bullet.GetComponent<NetworkObject>().Spawn(true);
        bullet.SetVelocityRpc(rigidbody2D.velocity + (Vector2)transform.right * 10f);
    }
}