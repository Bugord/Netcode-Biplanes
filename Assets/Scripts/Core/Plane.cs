using System;
using Unity.Netcode;
using UnityEngine;

public class Plane : NetworkBehaviour
{
    [SerializeField]
    private Animator animator;
    
    [SerializeField]
    private PlaneControl control;

    [SerializeField]
    private Health health;

    [SerializeField]
    private float edgeDistance;
    private static readonly int DieHash = Animator.StringToHash("Die");

    public override void OnNetworkSpawn()
    {
        name = $"Plane {(IsOwnedByServer ? "Server" : "Client")}";

        if (!IsServer) {
            return;
        }

        RespawnRpc();
        health.Died += DieRpc;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) {
            return;
        }

        health.Died -= DieRpc;
    }

    private void Update()
    {
        if (!IsOwner) {
            return;
        }
        
        var playerPos = transform.position;
        if (Mathf.Abs(playerPos.x) > edgeDistance) { 
            transform.position = new Vector3(playerPos.x > 0 ? -edgeDistance : edgeDistance, playerPos.y);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void DieRpc()
    {
        animator.SetTrigger(DieHash);
        GetComponent<Rigidbody2D>().isKinematic = true;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        if (IsServer) {
            Invoke(nameof(RespawnRpc), 0.5f);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void RespawnRpc()
    {
        GetComponent<Rigidbody2D>().isKinematic = false;
        SetSpawnPositionRpc();
        control.Reset();
        health.Reset();
    }

    private void SetSpawnPositionRpc()
    {
        transform.position = NetworkObject.IsOwnedByServer
            ? GameObject.Find("Host Spawn Point").transform.position
            : GameObject.Find("Client Spawn Point").transform.position;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!IsOwner) {
            return;
        }

        if (col.collider.CompareTag("House")) {
            DieRpc();
        }

        if (control.IsTakenOff && col.collider.CompareTag("Ground")) {
            DieRpc();
        }

        Debug.Log($"{name}: collided with {col.collider.name}");
    }
}