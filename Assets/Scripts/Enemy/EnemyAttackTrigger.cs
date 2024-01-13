using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{

    public delegate void PlayerHitCallback(PlayerHealth player);

    [RequireComponent(typeof(Sprite))]
    public class EnemyAttackTrigger : MonoBehaviour
    {
        public string playerTag = "Player";

        public bool canHitMultipleTimes = false;
        public float hitFrequencyTime = 0.3f;
        private float _timeUntilNextAttackTime;
        
        private PlayerHitCallback onHitCallback;
        private bool hasHitPlayer;
        private SpriteRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (hasHitPlayer && canHitMultipleTimes)
            {
                _timeUntilNextAttackTime -= Time.deltaTime;
                if (_timeUntilNextAttackTime <= 0)
                {
                    hasHitPlayer = false;
                }
            }
        }

        private void OnEnable()
        {
            hasHitPlayer = false;

            SetRendererVisibility(GameManager.Instance.ShowAttackTriggers);
            GameManager.Instance.OnShowAttackTriggerChange += SetRendererVisibility;
        }

        private void OnDisable()
        {
            GameManager.Instance.OnShowAttackTriggerChange -= SetRendererVisibility;
        }

        public void SetRendererVisibility(bool visible)
        {
            _renderer.enabled = visible;
        }


        private void OnTriggerStay2D(Collider2D col)
        {
            //dont hit an enemy twice
            if (hasHitPlayer)
            {
                Debug.Log("already hit: " + col.tag);
                return;
            }

            Debug.Log("Tag of GO that attackTrigger hit: " + col.tag);
            if (!col.CompareTag(playerTag)) return;


            PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogWarning("GameObject with \"Player\" tag doesnt have PlayerHealth component!");
                return;
            }

            onHitCallback(playerHealth);
            
            hasHitPlayer = true;
            if (canHitMultipleTimes)
            {
                _timeUntilNextAttackTime = hitFrequencyTime;
            }
        }

        public void RegisterOnHit(PlayerHitCallback callback)
        {
            onHitCallback = callback;
        }
    }

}