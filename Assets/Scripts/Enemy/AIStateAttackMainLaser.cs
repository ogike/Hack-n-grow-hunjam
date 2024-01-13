using System;
using Player;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemy.States
{
    [System.Serializable]
    public class AIStateAttackMainLaser : AIStateAttackMain
    {
        protected override string stateDebugName => "Attack Main Laser";

        [Header("Laser values")]
        public float fastTrackingTime;
        public float fastTrackingSpeed;
        [FormerlySerializedAs("trackingSpeed")] public float slowTrackingSpeed;
        private float _curTrackingSpeed;
        
        public float trackingMaxRange;
        public float startingLaserLength;

        public float raycastTickTime = 0.1f;
        private float _curRaycastTickTime;
        public LayerMask attackLayerMask;

        [Header("References")]
        public Transform trackingStartPos;
        public GameObject laserEndEffect;
        private Transform _laserEndEffectTransform;

        public LineRenderer lineRenderer;

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
            lineRenderer.enabled = false;

            if (startingLaserLength > trackingMaxRange)
            {
                Debug.LogWarning("Starting laser length is longer than max range!");
            }
        }

        public override void Entry()
        {
            base.Entry();

            Vector3 startPos = trackingStartPos.position;
            _trackingPos = Vector3.MoveTowards(startPos, _playerTrans.position, startingLaserLength);
            
            _laserEndEffectTransform.position = _trackingPos;
            laserEndEffect.SetActive(true);

            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, _trackingPos);

            _curRaycastTickTime = 0;
            _curTrackingSpeed = fastTrackingSpeed;
        }


        public override void Tick()
        {
            base.Tick();

            if (curTimeSinceEntry >= fastTrackingTime) _curTrackingSpeed = slowTrackingSpeed;
            
            _trackingPos = Vector3.MoveTowards(_trackingPos, _playerTrans.position,
                _curTrackingSpeed * Time.deltaTime);

            if (Vector2.Distance(trackingStartPos.position, _trackingPos) > trackingMaxRange)
            {
                _trackingPos = Vector3.MoveTowards(trackingStartPos.position, _playerTrans.position, trackingMaxRange);
            }
            
            _laserEndEffectTransform.position = _trackingPos;
            
            lineRenderer.SetPosition(0, trackingStartPos.position);
            lineRenderer.SetPosition(1, _trackingPos);

            if (_curRaycastTickTime <= 0)
            {
                RaycastCheck();
                _curRaycastTickTime = raycastTickTime;
            }
            else
            {
                _curRaycastTickTime -= Time.deltaTime;
            }
        }

        public void RaycastCheck()
        {
            Vector3 startPos = trackingStartPos.position;
            Vector3 endPos = _trackingPos;
            float dist = Vector3.Distance(startPos, endPos);

            RaycastHit2D hit = Physics2D.Raycast(
                startPos, endPos, 
                dist, attackLayerMask
                );

            if (hit)
            {
                PlayerHealth playerHealth = hit.transform.GetComponent<PlayerHealth>();
                if (playerHealth == null)
                {
                    Debug.LogWarning("Raycast hit result "  + hit.transform.name + "doesnt have PlayerHealth component!");
                    return;
                }

                base.AttackOnHit(playerHealth);
            }
        }

        public override void Exit()
        {
            laserEndEffect.SetActive(false);
            lineRenderer.enabled = false;
        }
    }
}
