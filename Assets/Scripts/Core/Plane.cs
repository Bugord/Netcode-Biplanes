using System;
using Core;
using Unity.Netcode;
using UnityEditor;
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
    private GameObject smokeParticles;
    
    [SerializeField]
    private GameObject fireParticles;

    private float edgeDistance;
    private Vector3 spawnPosition;

    private static readonly int IsAliveHash = Animator.StringToHash("IsAlive");

    public Team Team { get; private set; }

    private void Awake()
    {
    }

    public override void OnNetworkSpawn()
    {
        name = $"Plane {(IsOwnedByServer ? "Server" : "Client")}";

        health.HealthChanged += OnHealthChanged;
            
        if (!IsServer) {
            return;
        }

        health.Died += DieRpc;
    }

    public override void OnNetworkDespawn()
    {
        health.HealthChanged -= OnHealthChanged;
        
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

        if (Mathf.Abs(transform.position.x) > edgeDistance) {
            TeleportToOtherSideRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    public void InitRpc(Team team, Vector3 spawnPosition, float edgeDistance)
    {
        Team = team;
        this.spawnPosition = spawnPosition;
        this.edgeDistance = edgeDistance;

        RespawnRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void RespawnRpc()
    {
        transform.position = spawnPosition;
        control.enabled = true;

        animator.SetBool(IsAliveHash, true);
        ResetComponents();
    }

    [Rpc(SendTo.Everyone)]
    private void DieRpc()
    {
        control.enabled = false;
        
        animator.SetBool(IsAliveHash, false);
        Crashed?.Invoke(this, health.CurrentHealth == 0 ? PlaneCrashReason.Destroyed : PlaneCrashReason.Suicide);
    }

    [Rpc(SendTo.Everyone)]
    private void TeleportToOtherSideRpc()
    {
        var playerPos = transform.position;
        transform.position = new Vector3(playerPos.x > 0 ? -edgeDistance : edgeDistance, playerPos.y);
    }

    public void Respawn()
    {
        RespawnRpc();
    }

    private void ResetComponents()
    {
        control.Reset();
        health.Reset();
        UpdateParticlesState();
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

    private void OnHealthChanged(int current)
    {
        UpdateParticlesState();
    }

    private void UpdateParticlesState()
    {
        var currentHealth = health.CurrentHealth;
        smokeParticles.SetActive(currentHealth == 2);
        fireParticles.SetActive(currentHealth == 1);
    }
}