using System;
using System.Collections;
using System.Collections.Generic;
using Enemy.States;
using UnityEngine;
using UnityEngine.Serialization;
using Enemy.States;


namespace Enemy
{
    public class EnemyAI : MonoBehaviour
    {
        private AIState curState;
        //useful for debugging
        String curStateName;
        protected List<AIState> states;
        
        //transitions that will happen automatically on finish of state
        protected Dictionary<AIState, AIState> defaultTransitions; 
        
        //TODO: this should be in the derived class
        public float attackRange;
        
        private float _curAttackCooldown;

        [Header("Moving")] 
        public AIStateMove stateMove;
        
        [Header("Attacking")]
        public AIStateAttackWindup stateAttackWindup;
        public AIStateAttackMain stateAttackMain;
        public AIStateAttackWinddown stateAttackWinddown;
        
        [Header("Knockback")]
        public AIStateStateKnockBack stateKnockBack;
        
        public Animator animator;

        //private refs, state vars
        protected Transform _playerTrans;
        protected Rigidbody2D _myRigid;
        protected GameManager _gameManager;

        //Update() cached variables
        public Vector2 DirToPlayer { get; private set; }
        public float DistanceToPlayer { get; private set; }

        private void Awake()
        {
            states = new List<AIState>
            {
                stateMove,
                stateAttackWindup,
                stateAttackMain,
                stateAttackWinddown,
                stateKnockBack
            };

            foreach (AIState state in states)
            {
                state.Setup(this);
            }

            defaultTransitions = new Dictionary<AIState, AIState>();
            AddDefaultTransition(stateAttackWindup, stateAttackMain);
            AddDefaultTransition(stateAttackMain, stateAttackWinddown);
            AddDefaultTransition(stateAttackWinddown, stateMove);
            
            AddDefaultTransition(stateKnockBack, stateMove);
            
            ChangeState(stateMove);
        }

        void Start()
        {
            _playerTrans = PlayerController.Instance.transform;
            _gameManager = GameManager.Instance;
            _myRigid = GetComponent<Rigidbody2D>();
            if (_myRigid == null)
            {
                Debug.LogError("No rigidbody2D attached to this enemy!");
            }

            if (animator == null)
            {
                Debug.LogError("No animator attached to this enemy!");
            }
            
            stateAttackWindup.Init();

            //this is a hack - has to be replaced by triggers
            attackRange *= PlayerController.Instance.transform.localScale.x;
            
            //TODO: derived class
            // _state = EnemyState.Moving;
            foreach (AIState state in states)
            {
                state.Init();
            }
        }

        void Update()
        {
            //setting up cached vars
            Vector2 myPos = transform.position;
            Vector2 playerPos = _playerTrans.position;

            DirToPlayer = playerPos - myPos;
            DistanceToPlayer = DirToPlayer.magnitude;
            DirToPlayer = DirToPlayer.normalized;

            //state machine
            CheckTransitions();
            curState.Tick();
        }

        public void MoveRigidbody(Vector2 force)
        {
            _myRigid.AddForce(force);
        }

        public void ResetAttackTriggers()
        {
            //reset animator triggers too
            AnimatorResetTrigger("AttackState");
            AnimatorResetTrigger("AttackMain");
            AnimatorResetTrigger("AttackWinddown");
            AnimatorResetTrigger("AttackExit");
        }

        public void KnockBack(float knockoutTime)
        {
            stateKnockBack.SetKnockbackAmount(knockoutTime);
            ChangeState(stateKnockBack);
            
            

            //cancel any ongoing attack
            ResetAttackTriggers();

            //put effects here
        }

        public virtual void CheckTransitions()
        {
            if (_curAttackCooldown > 0)
            {
                _curAttackCooldown -= Time.deltaTime;
            }

            switch (curState)
            {
                case AIStateMove:
                    if (DistanceToPlayer < attackRange && CanAttack())
                    {
                        ChangeState(stateAttackWindup);
                    }
                    break;
                default:
                    break;
            }
        }

        public bool CanAttack()
        {
            if (!_gameManager.EnemyAttackEnabled) return false;

            if (_curAttackCooldown > 0) return false;
            
            return true;
        }

        public void SetAttackCooldownTime(float newCooldown)
        {
            _curAttackCooldown = newCooldown;
        }

        public void AddDefaultTransition(AIState from, AIState to)
        {
            defaultTransitions.Add(from, to);
        }

        /// <summary>
        /// Finished the current state passed, and changes state if there is a default transition set
        /// </summary>
        /// <param name="state">The state to finish.</param>
        public void FinishState(AIState state)
        {
            if (defaultTransitions.ContainsKey(state))
            {
                ChangeState(defaultTransitions[state]);
            }
            else
            {
                Debug.LogWarning("State finished without next default state set up!");
            }
        }

        public void ChangeState(AIState to)
        {
            curState?.Exit();
            curState = to;
            to.Entry();
            // Debug.Log("Changing state from " + curStateName + " to " + to);
            curStateName = to.ToString();
        }
        
        public void AnimatorSetTrigger(string name)
        {
            if (animator == null)           return;
            if (string.IsNullOrEmpty(name)) return;

            animator.SetTrigger(name);
        }
        
        public void AnimatorResetTrigger(string name)
        {
            if (animator == null)           return;
            if (string.IsNullOrEmpty(name)) return;

            animator.ResetTrigger(name);
        }

        public void AnimatorSetBool(string name, bool value)
        {
            if (animator == null)           return;
            if (string.IsNullOrEmpty(name)) return;
		
            animator.SetBool(name, value);
        }

        public void AnimatorSetFloat(string name, float value)
        {
            if (animator == null)           return;
            if (string.IsNullOrEmpty(name)) return;
		
            animator.SetFloat(name, value);
        }
    }
}