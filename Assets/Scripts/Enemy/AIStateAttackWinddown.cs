using System;
using UnityEngine;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateAttackWinddown : AIState
    {
        protected override string stateDebugName => "Attack Winddown";

        public AnimationClip attackWindDownReferenceAnim;

        public float attackCooldownTime;
        
        public override void Entry()
        {
            base.Entry();
            
            _controller.AnimatorSetFloat("AttackWinddownTime",
                attackWindDownReferenceAnim.length / animationInfo.stateTime);
        }

        public override void Exit()
        {
            base.Exit();

            _controller.SetAttackCooldownTime(attackCooldownTime);
            _controller.AnimatorSetTrigger("AttackExit");
        }
    }
}
