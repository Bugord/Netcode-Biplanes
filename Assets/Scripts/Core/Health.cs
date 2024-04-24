using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    public event Action Died;

    [SerializeField]
    private int maxHealthpoints;

    private readonly NetworkVariable<int> healthpoints = new NetworkVariable<int>();
    
    public void Reset()
    {
        if (!IsServer) {
            return;
        }
        
        healthpoints.Value = maxHealthpoints;
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