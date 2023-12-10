using System;
using UnityEngine;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateMove : AIState
    {
        public float speed = 5;
        public float rotationSpeed = 5;

        /// <summary>
        /// Called every frame by the state machine.
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            
            if(!_gameManager.EnemyMovementEnabled) return;

            _controller.RotateTowardsDir(_controller.DirToPlayer, rotationSpeed);
            
            Vector2 newForce = _controller.DirForward * (speed * Time.deltaTime);
            _controller.MoveRigidbody(newForce);
        }
    }
}
