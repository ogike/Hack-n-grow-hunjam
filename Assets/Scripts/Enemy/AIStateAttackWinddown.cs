using System;
using UnityEngine;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateAttackWinddown : AIState
    {
        public AnimationClip attackWindDownReferenceAnim;

        public float attackCooldownTime;
        
        public override void Entry()
        {
            base.Entry();
            
            _controller.AnimatorSetFloat("AttackWinddownTime",
                attackWindDownReferenceAnim.length / exitTime);
        }

        public override void Exit()
        {
            base.Exit();

            _controller.SetAttackCooldownTime(attackCooldownTime);
            _controller.AnimatorSetTrigger("AttackExit");
        }
    }
}
