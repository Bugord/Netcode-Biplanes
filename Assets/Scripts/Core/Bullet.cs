using System.Collections;
using Core;
using Unity.Netcode;
using UnityEngine;

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

    [Rpc(SendTo.NotServer)]
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

        if (TryGetComponent<NetworkedPlaneController>(out var networkedPlaneController)) {
            if (OwnerClientId == networkedPlaneController.OwnerClientId) {
                return;
            }
        }
        
        if (col.TryGetComponent<Health>(out var health)) {
            health.TakeDamage();
        }

        NetworkObject.Despawn();
    }
}