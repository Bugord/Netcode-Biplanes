﻿using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    public event Action Died;
    public event Action<int> HealthChanged;

    [SerializeField]
    private int maxHealthpoints;

    public int CurrentHealth => healthpoints.Value;

    private NetworkVariable<int> healthpoints;

    private void Awake()
    {
        healthpoints = new NetworkVariable<int>(maxHealthpoints);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) {
            enabled = false;
        }
        
        healthpoints.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        healthpoints.OnValueChanged -= OnHealthChanged;
    }

    public void Reset()
    {
        if (!IsServer) {
            return;
        }

        healthpoints.Value = maxHealthpoints;
    }

    private void OnHealthChanged(int previous, int current)
    {
        HealthChanged?.Invoke(current);

        if (current == 0) {
            Died?.Invoke();
        }
    }

    public void TakeDamage()
    {
        if (!IsServer) {
            return;
        }

        healthpoints.Value -= 1;
    }
}