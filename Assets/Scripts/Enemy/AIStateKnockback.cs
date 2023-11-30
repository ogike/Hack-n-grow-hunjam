using System;
using UnityEngine;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateStateKnockBack : AIState
    {
        
        public void SetKnockbackAmount(float amount)
        {
            exitTime = amount;
        }
        
    }
}
