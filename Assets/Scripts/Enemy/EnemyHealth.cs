using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health;
    private int curHealth;

    public int xpDrop;

    public AudioClip enemyDamageAudio;
    public AudioClip enemyDieAudio;

    private EnemyAI _enemyAI;
    private Rigidbody2D _rigidbody;
    
    

    void Start()
    {
        curHealth = health;
        _enemyAI = GetComponent<EnemyAI>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void Damage(int damage)
    {
        AudioManager.Instance.PlayAudio(enemyDamageAudio);
        curHealth -= damage;

        if (curHealth <= 0)
        {
            Die();
        }
    }

    public void Knockback(float knockoutTime, Vector2 knockbackForce)
    {
        _rigidbody.AddForce(knockbackForce);
        _enemyAI.KnockBack(knockoutTime);
    }

    public void Die()
    {
        AudioManager.Instance.PlayAudio(enemyDieAudio);
        EnemySpawner.Instance.DecreaseEnemyCount();
        PlayerController.Instance.AddXp(xpDrop);
        
        GameObject.Destroy(gameObject);
    }
}
