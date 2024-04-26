using System;
using Core;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Plane : NetworkBehaviour
{
    public event Action<Plane, PlaneCrashReason> Crashed;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private PlaneControl control;

    [SerializeField]
    private Health health;

    [SerializeField]
    private float edgeDistance;

    public Team Team { get; private set; }

    private Vector3 spawnPosition;

    private static readonly int DieHash = Animator.StringToHash("Die");

    public override void OnNetworkSpawn()
    {
        name = $"Plane {(IsOwnedByServer ? "Server" : "Client")}";

        if (!IsServer) {
            return;
        }

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
    public void InitRpc(Team team, Vector3 spawnPosition)
    {
        Team = team;
        this.spawnPosition = spawnPosition;
        
        RespawnRpc();
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

        Crashed?.Invoke(this, health.CurrentHealth == 0 ? PlaneCrashReason.Destroyed : PlaneCrashReason.Suicide);
    }

    [Rpc(SendTo.Everyone)]
    public void RespawnRpc()
    {
        GetComponent<Rigidbody2D>().isKinematic = false;
        transform.position = spawnPosition;
        control.Reset();
        health.Reset();
    }

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        if (!IsOwner) {
            return;
        }

        if (collision2D.collider.CompareTag("House")) {
            DieRpc();
        }

        if (control.IsTakenOff && collision2D.collider.CompareTag("Ground")) {
            DieRpc();
        }

        Debug.Log($"{name}: collided with {collision2D.collider.name}");
    }
}