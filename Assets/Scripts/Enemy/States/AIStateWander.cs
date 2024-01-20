using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateWander : AIState
    {
        protected override string stateDebugName => "Wandering";
        
        public float speed = 5;
        public float rotationSpeed = 5;
        
        public float changeDirectionTimeMin;
        public float changeDirectionTimeMax;
        private float _timeUntilNextDirectionChange;

        private Vector2 _curDir;

        public override void Entry()
        {
            base.Entry();
            
            ChooseNewDir();
        }

        /// <summary>
        /// Called every frame by the state machine.
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            
            if(!_gameManager.EnemyMovementEnabled) return;

            if (_timeUntilNextDirectionChange > 0)
            {
                _timeUntilNextDirectionChange -= Time.deltaTime;
            } else
            {
                ChooseNewDir();  
            }

            _controller.RotateTowardsDir(_curDir, rotationSpeed);
            
            Vector2 newForce = _curDir * (speed * Time.deltaTime);
            _controller.MoveRigidbody(newForce);
        }

        private void ChooseNewDir()
        {
            float minX = -1, minY = -1;
            float maxX = 1,  maxY = 1;

            if (_controller.MyPosition.x > Map.Instance.MaxPosition.x) maxX = -0.2f;
            if (_controller.MyPosition.x < Map.Instance.MinPosition.x) minX = 0.2f;
            
            if (_controller.MyPosition.y > Map.Instance.MaxPosition.y) maxY = -0.2f;
            if (_controller.MyPosition.y < Map.Instance.MinPosition.y) minY = 0.2f;
            
            
            _curDir = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY)).normalized;

            _timeUntilNextDirectionChange = Random.Range(changeDirectionTimeMin, changeDirectionTimeMax);
        }
    }
}
