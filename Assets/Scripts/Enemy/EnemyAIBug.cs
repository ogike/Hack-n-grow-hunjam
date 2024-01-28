using System.Collections.Generic;
using Enemy.States;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemy
{
    public class EnemyAIBug : EnemyAI
    {
        public float rangeToStartAttack = 1.5f;
        public float rangeToStartChasing = 3.0f;
        
        [Header("Moving")] 
        [FormerlySerializedAs("stateMove")] 
        public AIStateMove stateChase;
        public AIStateWander stateWander;

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
                stateChase,
                stateWander,
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
            AddDefaultTransition(stateAttackWinddown, stateChase);

            AddDefaultTransition(stateKnockBack, stateChase);
        }

        protected override void SetToDefaultState()
        {
            ChangeState(stateWander);
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
                    if (DistanceToPlayer < rangeToStartAttack && CanAttack())
                        ChangeState(stateAttackWindup);
                    else if(DistanceToPlayer > rangeToStartChasing && _canWander)
                        ChangeState(stateWander);
                    break;
                case AIStateWander:
                    if(!_canWander || DistanceToPlayer < rangeToStartChasing)
                        ChangeState(stateChase);
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
        
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (_myTrans == null) _myTrans = transform;
            
            Vector3 myPos = _myTrans.position;
            
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireDisc(myPos ,Vector3.back, rangeToStartChasing);
            
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawWireDisc(myPos,Vector3.back, rangeToStartAttack);
        }
#endif //UNITY_EDITOR

    }
}
