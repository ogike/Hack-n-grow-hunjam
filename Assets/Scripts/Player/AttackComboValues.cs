using System;
using UnityEngine;
using static Player.PlayerController;

namespace Player
{
    [Serializable]
    public class AttackComboValues
    {
        public float damage;
        public TimeValue knockoutTime;
        public float knockoutForce;
    
        [Header("Timing")]
        public TimeValue anticipationTime;
        public TimeValue strikeTime;
        public TimeValue recoveryTime;
        public TimeValue recoveryReadiedTime;
        public TimeValue cooldownTime;

        public AttackMovementRestriction anticipationRestriction;
        public AttackMovementRestriction strikeRestriction;
        public AttackMovementRestriction recoveryRestriction;
        
        // [Tooltip("Can the attack be cancelled by a dash")]
        // public bool dashCancelable;
        
        [Header("Reference animations")]
        public AnimationClip anticipationReferenceAnim;
        public AnimationClip strikeReferenceAnim;
        public AnimationClip recoveryReferenceAnim;
        public AnimationClip recoveryReadiedReferenceAnim;

        [Header("Effects")]
        public AudioClip strikeAudio;
        public GameObject strikeEffectSprite; //enabled when trigger is active
    }
}
