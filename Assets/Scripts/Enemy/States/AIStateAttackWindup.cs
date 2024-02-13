using System;
using UnityEngine;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateAttackWindup : AIState
    {
        protected override string stateDebugName => "Attack Windup";

        public AnimationClip attackWindUpReferenceAnim;
        public GameObject attackFlash;


        public float rotateSpeed;
        
        
        public override void Entry()
        {
            base.Entry();
            
            ParticleManager.Instance.SpawnParticleAt(attackFlash, _controller.MyPosition);
            _controller.AnimatorSetFloat("AttackWindupTime",
                attackWindUpReferenceAnim.length / animationInfo.stateTime.Seconds);
        }

        public override void Tick()
        {
            base.Tick();
            
            _controller.RotateTowardsDir(_controller.DirToPlayer, rotateSpeed);
        }
    }
}
