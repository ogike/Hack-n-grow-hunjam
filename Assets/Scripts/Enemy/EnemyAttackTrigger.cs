using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{

    public delegate void PlayerHitCallback(PlayerHealth player);

    public class EnemyAttackTrigger : MonoBehaviour
    {
        public string playerTag = "Player";

        private PlayerHitCallback onHitCallback;
        private bool hasHitPlayer;

        private void OnEnable()
        {
            hasHitPlayer = false;
            Debug.Log("AttackTrigger OnEnable()");
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
                Debug.LogWarning("GameObject with \"Player\" tag doesnt have EnemyHealth component!");
                return;
            }

            onHitCallback(playerHealth);
            hasHitPlayer = true;
        }

        public void RegisterOnHit(PlayerHitCallback callback)
        {
            onHitCallback = callback;
        }
    }

}