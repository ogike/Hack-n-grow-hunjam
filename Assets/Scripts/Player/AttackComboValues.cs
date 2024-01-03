using System;
using UnityEngine;
using static Player.PlayerController;

namespace Player
{
    [Serializable]
    public class AttackComboValues
    {
        public float damage;
        public float knockoutTime;
        public float knockoutForce;
    
        [Header("Timing")]
        public float anticipationTime;
        public float strikeTime;
        public float recoveryTime = 0.1f;
        public float recoveryReadiedTime = 0.5f;
        public float cooldownTime;

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
