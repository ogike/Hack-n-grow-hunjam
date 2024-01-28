using System;
using TMPro.SpriteAssetUtilities;
using UnityEngine;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateStateKnockBack : AIState
    {
        protected override string stateDebugName => "Knockback";

        public void SetKnockbackAmount(float amount)
        {
            animationInfo.stateTime.frames = Mathf.FloorToInt(amount / FrameTime.FrameTimeSeconds);
        }
        
    }
}
