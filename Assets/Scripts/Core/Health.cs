using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    public event Action Died;
    public event Action<int, int> HealthChanged;

    [SerializeField]
    private int maxHealthpoints;

    public int CurrentHealth => healthpoints.Value;

    private readonly NetworkVariable<int> healthpoints = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
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
        HealthChanged?.Invoke(previous, current);
    }

    public void TakeDamage()
    {
        if (!IsServer) {
            return;
        }

        healthpoints.Value -= 1;
        
        if (healthpoints.Value == 0) {
            Died?.Invoke();
        }
    }
}