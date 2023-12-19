using System;
using UnityEngine;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateStateKnockBack : AIState
    {
        protected override string stateDebugName => "Knockback";

        public void SetKnockbackAmount(float amount)
        {
            animationInfo.stateTime = amount;
        }
        
    }
}
