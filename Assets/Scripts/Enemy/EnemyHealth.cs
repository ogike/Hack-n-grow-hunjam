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

        public GameObject hitParticle;
        public GameObject deathParticle;

        private EnemyAI _enemyAI;
        private Rigidbody2D _rigidbody;
        private Transform _transform;
        private Animator _animator;


        void Start()
        {
            curHealth = health;
            _enemyAI = GetComponent<EnemyAI>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _transform = transform;
            _animator = _enemyAI.animator;
        }

        public void Damage(int damage)
        {
            AudioManager.Instance.PlayAudio(enemyDamageAudio);
            _animator.SetTrigger("Damaged");
            Vector3 directionFrom = _enemyAI.DirToPlayer;
            
            ParticleManager.Instance.SpawnParticleWithLookDirection(hitParticle, _transform.position, -directionFrom);

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
            
            ParticleManager.Instance.SpawnParticleWithLookDirection(deathParticle, _transform.position, Vector3.zero);

            GameObject.Destroy(gameObject);
        }
    }

}