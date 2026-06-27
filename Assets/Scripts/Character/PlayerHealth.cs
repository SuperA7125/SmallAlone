using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{

    public event Action Died;
    public event Action Respawned;

    public void TakeDamage()
    {
        Died?.Invoke();
    }

    public void RespawnPlayer()
    {
        CheckpointManager.Instance.Respawn();
        Respawned?.Invoke();
    }
}
