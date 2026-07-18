using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{

    public event Action Died;
    public event Action Respawned;
    public bool CanDie;

    public void TakeDamage()
    {
        //if (CanDie)
        //    //Died?.Invoke();
    }

    public void RespawnPlayer()
    {
        CheckpointManager.Instance.Respawn();
        Respawned?.Invoke();
    }
}
