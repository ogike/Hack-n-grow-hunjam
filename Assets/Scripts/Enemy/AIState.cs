using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemy.States
{
    public enum AnimatorEntryEventType { None, Bool, Trigger }
    
    [Serializable]
    public class AIStateMecanimInfo
    {

        public bool exitsByDefault;
        public float stateTime;
        
        [FormerlySerializedAs("AnimatorEntryTrigger")] [Tooltip("The Animator trigger or bool that will be set off on state entry")]
        public String animatorEntryTrigger;

        public AnimatorEntryEventType animatorEntryEventType;
    }
    
    [System.Serializable]
    public abstract class AIState
    {
        public AIStateMecanimInfo animationInfo;
        
        protected virtual string stateDebugName => "Default state name";
        private float curTimeSinceEntry;


        // REFERENCES //////////////////////
        protected EnemyAI _controller;
        protected GameManager _gameManager;


        /// <summary>
        /// Called during the Awake() call of the EnemyAI script
        /// </summary>
        /// <param name="controller">The EnemyAi script of this enemy</param>
        public virtual void Setup(EnemyAI controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Called during the Init() method of the controller, when this enemy gets spawned
        /// </summary>
        public virtual void Init()
        {
            _gameManager = GameManager.Instance;
            return;
        }

        /// <summary>
        /// Called as soon as the State Machine enters this state.
        /// Does nothing by default.
        /// </summary>
        public virtual void Entry()
        {
            curTimeSinceEntry = 0;
            
            if(string.IsNullOrEmpty(animationInfo.animatorEntryTrigger)) return;
            if(animationInfo.animatorEntryEventType == AnimatorEntryEventType.Bool)
                _controller.AnimatorSetBool(animationInfo.animatorEntryTrigger, true);
            else if(animationInfo.animatorEntryEventType == AnimatorEntryEventType.Trigger)
                _controller.AnimatorSetTrigger(animationInfo.animatorEntryTrigger);
        }

        /// <summary>
        /// Called every frame by the state machine.
        /// </summary>
        public virtual void Tick()
        {
            CountTime();
        }

        /// <summary>
        /// Updates time spent in this state since Entry.
        /// Will exit this state if exitTime is reached;
        /// </summary>
        public void CountTime()
        {
            curTimeSinceEntry += Time.deltaTime;

            if (animationInfo.exitsByDefault && curTimeSinceEntry >= animationInfo.stateTime)
            {
                _controller.FinishState(this);
            }
        }

        /// <summary>
        /// Called right before the State Machine leaves this state.
        /// Does nothing by default.
        /// </summary>
        public virtual void Exit()
        {
            if(animationInfo.animatorEntryEventType == AnimatorEntryEventType.Bool)
                _controller.AnimatorSetBool(animationInfo.animatorEntryTrigger, false);
            return;
        }
        
        public override string ToString()
        {
            if(string.IsNullOrEmpty(stateDebugName))
                return base.ToString();
            return stateDebugName;
        }
    }
}
