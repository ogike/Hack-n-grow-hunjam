using System;
using Player;
using UnityEngine;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateAttackMainLaser : AIStateAttackMain
    {
        protected override string stateDebugName => "Attack Main Laser";

        public float trackingSpeed;
        public Transform trackingStartPos;
        public GameObject laserEndEffect;
        private Transform _laserEndEffectTransform;

        private Vector3 _trackingPos;

        private Rigidbody2D _rigidbody2D;
        private Transform _playerTrans;

        public override void Init()
        {
            base.Init();

            _rigidbody2D = _controller.GetComponent<Rigidbody2D>();
            if (_rigidbody2D == null)
            {
                Debug.LogError("No rigidbody attached to given controller!");
            }

            _playerTrans = PlayerController.Instance.transform;
            _laserEndEffectTransform = laserEndEffect.transform;
            laserEndEffect.SetActive(false);
        }

        public override void Entry()
        {
            base.Entry();

            _trackingPos = trackingStartPos.position;
            _laserEndEffectTransform.position = _trackingPos;
            laserEndEffect.SetActive(true);
        }


        public override void Tick()
        {
            base.Tick();

            _trackingPos = Vector3.MoveTowards(_trackingPos, _playerTrans.position,
                trackingSpeed * Time.deltaTime);
            _laserEndEffectTransform.position = _trackingPos;
        }

        public override void Exit()
        {
            laserEndEffect.SetActive(false);   
        }
    }
}
