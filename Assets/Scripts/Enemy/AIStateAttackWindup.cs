using System;
using UnityEngine;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateAttackWindup : AIState
    {

        public AnimationClip attackWindUpReferenceAnim;

        
        public override void Entry()
        {
            base.Entry();
            
            _controller.AnimatorSetFloat("AttackWindupTime",
                attackWindUpReferenceAnim.length / exitTime);
        }

    }
}
