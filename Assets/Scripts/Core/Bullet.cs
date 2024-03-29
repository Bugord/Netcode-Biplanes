using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody2D rigidbody2D;

    private const float Lifetime = 5f;

    public void Config(Vector2 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        Invoke(nameof(DestroyBullet), Lifetime);
    }

    [Rpc(SendTo.Everyone)]
    public void SetVelocityRpc(Vector2 velocity)
    {
        rigidbody2D.velocity = velocity;
    }

    private void DestroyBullet()
    {
        NetworkObject.Despawn();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<Health>(out var health)) {
            health.TakeDamage();
        }

        NetworkObject.Despawn();
    }
}