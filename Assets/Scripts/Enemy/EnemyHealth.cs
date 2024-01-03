using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace Enemy
{
    public class EnemyHealth : MonoBehaviour
    {
        public int health;
        private int curHealth;

        public int xpDrop;

        public AudioClip enemyDamageAudio;
        public AudioClip enemyDieAudio;

        private EnemyAI _enemyAI;
        private Rigidbody2D _rigidbody;
        private Animator _animator;


        void Start()
        {
            curHealth = health;
            _enemyAI = GetComponent<EnemyAI>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = _enemyAI.animator;
        }

        public void Damage(int damage)
        {
            AudioManager.Instance.PlayAudio(enemyDamageAudio);
            _animator.SetTrigger("Damaged");

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
            if (EnemySpawner.Instance != null)
                EnemySpawner.Instance.DecreaseEnemyCount();
            PlayerController.Instance.AddXp(xpDrop);

            GameObject.Destroy(gameObject);
        }
    }

}