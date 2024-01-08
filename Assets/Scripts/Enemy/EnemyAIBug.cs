using System.Collections.Generic;
using Enemy.States;
using UnityEngine;

namespace Enemy
{
    public class EnemyAIBug : EnemyAI
    {
        [Header("Moving")] 
        public AIStateMove stateMove;
        
        [Header("Attacking")]
        public AIStateAttackWindup stateAttackWindup;
        public AIStateAttackMainMoving stateAttackMain;
        public AIStateAttackWinddown stateAttackWinddown;
        
        [Header("Knockback")]
        public AIStateStateKnockBack stateKnockBack;


        protected override void CreateStates()
        {
            states = new List<AIState>
            {
                stateMove,
                stateAttackWindup,
                stateAttackMain,
                stateAttackWinddown,
                stateKnockBack
            };
        }

        protected override void SetDefaultTransitions()
        {
            AddDefaultTransition(stateAttackWindup, stateAttackMain);
            AddDefaultTransition(stateAttackMain, stateAttackWinddown);
            AddDefaultTransition(stateAttackWinddown, stateMove);

            AddDefaultTransition(stateKnockBack, stateMove);
        }

        protected override void SetToDefaultState()
        {
            ChangeState(stateMove);
        }

        public override void KnockBack(float knockoutTime)
        {
            stateKnockBack.SetKnockbackAmount(knockoutTime);
            ChangeState(stateKnockBack);
            
            //cancel any ongoing attack
            ResetAttackTriggers();

            //put effects here
        }

        public override void CheckTransitions()
        {

            switch (curState)
            {
                case AIStateMove:
                    if (DistanceToPlayer < stateAttackMain.rangeToStartAttack && CanAttack())
                    {
                        ChangeState(stateAttackWindup);
                    }
                    break;
                default:
                    break;
            }
        }

        public void ResetAttackTriggers()
        {
            //reset animator triggers too
            AnimatorResetTrigger("AttackState");
            AnimatorResetTrigger("AttackMain");
            AnimatorResetTrigger("AttackWinddown");
            AnimatorResetTrigger("AttackExit");
        }
        
        public bool CanAttack()
        {
            if (!_gameManager.EnemyAttackEnabled) return false;

            if (_curAttackCooldown > 0) return false;
            
            return true;
        }
        
    }
}
