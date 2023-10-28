using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health;
    private int curHealth;

    public AudioClip enemyDamageAudio;
    public AudioClip enemyDieAudio;

    void Start()
    {
        curHealth = health;
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

    public void Die()
    {
        AudioManager.Instance.PlayAudio(enemyDieAudio);
        GameObject.Destroy(gameObject);
    }
}
