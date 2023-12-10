using System;
using UnityEngine;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateAttackWindup : AIState
    {

        public AnimationClip attackWindUpReferenceAnim;

        public float rotateSpeed;
        
        
        public override void Entry()
        {
            base.Entry();
            
            _controller.AnimatorSetFloat("AttackWindupTime",
                attackWindUpReferenceAnim.length / exitTime);
        }

        public override void Tick()
        {
            base.Tick();
            
            _controller.RotateTowardsDir(_controller.DirToPlayer, rotateSpeed);
        }
    }
}
