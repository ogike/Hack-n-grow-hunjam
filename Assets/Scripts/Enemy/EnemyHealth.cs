using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health;
    private int curHealth;

    void Start()
    {
        curHealth = health;
    }

    public void Damage(int damage)
    {
        curHealth -= damage;

        if (curHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        GameObject.Destroy(gameObject);
    }
}
