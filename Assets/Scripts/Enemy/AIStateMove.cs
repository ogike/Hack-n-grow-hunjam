using System;
using UnityEngine;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateMove : AIState
    {
        public float speed = 5;
        

        /// <summary>
        /// Called every frame by the state machine.
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            
            if(!_gameManager.EnemyMovementEnabled) return;

            Vector2 newForce = _controller.DirToPlayer * (speed * Time.deltaTime); 
            
            _controller.AnimatorSetFloat("dirH", _controller.DirToPlayer.x);
            _controller.MoveRigidbody(newForce);
        }
    }
}
