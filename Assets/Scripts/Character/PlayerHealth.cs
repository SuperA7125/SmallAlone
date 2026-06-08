using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public void TakeDamage()
    {
        RespawnPlayer();
    }

    private void RespawnPlayer()
    {
        CheckpointManager.Instance.Respawn();
    }
}
