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
        Debug.Log("Player has taken damage and will respawn.");
        throw new NotImplementedException();
    }
}
