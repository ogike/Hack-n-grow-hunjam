using System;
using UnityEngine;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateAttackMain : AIState
    {
        [Header("Attack values")]
        public int attackDamage;
        public float attackKnockBackAmount = 1;
        
        //probably will be replaced by exiting the state...?
        //private IEnumerator _attackCoroutine;
        
        [Header("Sounds")] 
        public AudioClip attackAudio;
        
        [Header("References")] 
        public AnimationClip attackMainReferenceAnim;
        public EnemyAttackTrigger attackHitbox;
        
        private GameObject _attackHitboxGameObject;
        

        public override void Init()
        {
            base.Init();
            
            if (attackHitbox == null)
            {
                Debug.LogError("No attack hitbox/trigger attached to this enemy!");
            }

            attackHitbox.RegisterOnHit(AttackOnHit);
            _attackHitboxGameObject = attackHitbox.gameObject;
            _attackHitboxGameObject.SetActive(false);
        }

        public override void Entry()
        {
            base.Entry();
            
            _attackHitboxGameObject.SetActive(true);
            AudioManager.Instance.PlayAudio(attackAudio);
            
            _controller.AnimatorSetFloat("AttackMainTime",
                attackMainReferenceAnim.length / exitTime);
        }

        public override void Exit()
        {
            base.Exit();
            
            _attackHitboxGameObject.SetActive(false);
        }

        /// <summary>
        /// Called by the EnemyAttackTrigger hitbox component on collision
        /// </summary>
        public void AttackOnHit(PlayerHealth playerHealth)
        {
            //TODO: attack cancelling
            Debug.Log("Hitting player with: " + attackDamage);
            playerHealth.TakeDamage(attackDamage, 
                        _controller.DirForward * attackKnockBackAmount);

            //effects go here
        }
    }
}
