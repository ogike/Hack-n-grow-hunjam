using System;
using Player;
using UnityEngine;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateAttackMain : AIState
    {
        protected override string stateDebugName => "Attack Main";

        [Header("Attack values")]
        public int attackDamage;
        public float attackKnockBackAmount = 1;
        
        public bool stopOnSuccesfulHit;
        
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
                attackMainReferenceAnim.length / animationInfo.stateTime.Seconds);
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
            
            if(stopOnSuccesfulHit)
                _controller.FinishState(this);

            //effects go here
        }
    }
}
