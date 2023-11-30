using System;
using UnityEngine;

namespace Enemy.States
{
    [System.Serializable]
    public abstract class AIState
    {
        public string stateDebugName;
        
        protected EnemyAI _controller;

        [Header("Animation")]
        
        [Tooltip("The Animator trigger or bool that will be set off on state entry")]
        public String AnimatorEntryTrigger;

        [Tooltip("Whether the Animator condition is a bool or trigger")]
        public bool AnimatorEntryTriggerIsBool;

        /// <summary>
        /// Called during the Awake() call of the EnemyAI script
        /// </summary>
        /// <param name="controller">The EnemyAi script of this enemy</param>
        public abstract void Setup(EnemyAI controller);
        
        /// <summary>
        /// Called during the Init() method of the controller, when this enemy gets spawned
        /// </summary>
        public virtual void Init() {return;}

        /// <summary>
        /// Called as soon as the State Machine enters this state.
        /// Does nothing by default.
        /// </summary>
        public virtual void Entry()
        {
            if(AnimatorEntryTriggerIsBool)
                _controller.AnimatorSetBool(AnimatorEntryTrigger, true);
            else
                _controller.AnimatorSetTrigger(AnimatorEntryTrigger);
            return;
        }
        
        /// <summary>
        /// Called every frame by the state machine.
        /// </summary>
        public abstract void Tick();

        /// <summary>
        /// Called right before the State Machine leaves this state.
        /// Does nothing by default.
        /// </summary>
        public virtual void Exit()
        {
            if(AnimatorEntryTriggerIsBool)
                _controller.AnimatorSetBool(AnimatorEntryTrigger, false);
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
