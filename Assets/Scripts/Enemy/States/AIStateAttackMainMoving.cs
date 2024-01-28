using System;
using UnityEngine;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateAttackMainMoving : AIStateAttackMain
    {
        protected override string stateDebugName => "Attack Main Moving";

        public float speed;

        private Vector2 _dir;
        private Rigidbody2D _rigidbody2D;

        public override void Init()
        {
            base.Init();

            _rigidbody2D = _controller.GetComponent<Rigidbody2D>();
            if (_rigidbody2D == null)
            {
                Debug.LogError("No rigidbody attached to given controller!");
            }
        }

        public override void Entry()
        {
            base.Entry();
            
            _dir = _controller.DirForward;
        }


        public override void Tick()
        {
            base.Tick();

            Vector2 newForce = _dir * (speed * Time.deltaTime);
            _controller.MoveRigidbody(newForce);
        }
    }
}
