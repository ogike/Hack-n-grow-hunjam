using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void PlayerHitCallback(PlayerHealth player);

public class EnemyAttackTrigger : MonoBehaviour
{
    public string playerTag = "Player";
    
    private PlayerHitCallback onHitCallback;
    private bool hasHitPlayer;
    
    private void OnEnable()
    {
        hasHitPlayer = true;
    }
    
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(!col.CompareTag(playerTag)) return;

        //dont hit an enemy twice
        if (hasHitPlayer) return;

        PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("GameObject with \"Player\" tag doesnt have EnemyHealth component!");
            return;
        }

        onHitCallback(playerHealth);
    }

    public void RegisterOnHit(PlayerHitCallback callback)
    {
        onHitCallback = callback;
    }
}