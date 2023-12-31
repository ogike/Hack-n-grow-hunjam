using System;
using System.Collections;
using System.Collections.Generic;
using Enemy;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Moving")] //###############################################################################################
        public float baseSpeed = 15;

        [Tooltip("The force by which the player will be knocked back")]
        public float baseKnockBackedForce = 1;

        private float plusRotValue;

        private float lastInputH;
        private float lastInputV;
        private bool rotatedThisUpdate = false;

        private float _floatingTolerance = 0.001f;
    
        //starting level = 0
        [Header("Growth levels")] //########################################################################################
        public int maxLevel;
        public int size2Level = 3;

        public List<int> levelXpRequirements;
    
        public List<float> speedModifiers;
        public List<float> damageModifiers;
        public List<float> rangeModifiers;
        public List<float> cooldownModifiers;
        public List<float> knockbackModifiers;
        public List<float> transformSizeModifiers;
    
        private int curXp;
        public int CurLevel { get; private set; }

        public AudioClip growAudio;

        //Attack //#########################################################################################################

        public enum AttackState { NotAttacking, Windup, ActiveAttack, WinddownPre, WinddownReady, Cooldown }
        public enum AttackComboState { LeftSwing, RightSwing }
        public enum AttackMovementRestriction {None, Stop, HalfSpeed}

        public AttackState CurrentAttackState { get; private set; }
        private IEnumerator _activeAttackCoroutine;
        public AttackComboState CurrentAttackComboState { get; private set; }

        private bool hasPressedAttackThisCombo;

        [Header("Attacks")] //#########################################################################################
        // public float attackRanges; //TODO: remove this, instead of using it for scaling...?
        public AttackComboValues lightAttackLeftSwing;
        public AttackComboValues lightAttackRightSwing;
        private AttackComboValues _curAttack;
        

        public PlayerWeaponTrigger attackTrigger;
        private GameObject _attackTriggerGameObject;

        private Vector2 _last4WayDir;

        [Header("Dashing")] //##############################################################################################
        [HideInInspector]public float dashSpeed;
        [HideInInspector]public float dashActiveTime;
        [HideInInspector]public float dashCooldownTime;
        [HideInInspector]public float dashPostFreezeTime;

        [HideInInspector]public AudioClip dashAudio;

        public enum DashType {DashForward, QuickstepBack}
        public DashType dashType;
    
        private float curDashActiveLeft;
        private float curDashCooldownLeft;
        private float curDashFreezeLeft;

        [Header("UI")] //###################################################################################################
        [HideInInspector]public Image dashCooldownMeter;
        public Image xpMeter;
        public Text levelDisplayText;

        [Header("Animations")]  //##########################################################################################
        public Animator animator;


        [Header("References")] //#####################################################################################################
        public Transform playerSprite;
    
        private Transform _trans;
        private Rigidbody2D _rigidbody;
        private PlayerHealth _playerHealth;
    
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Multiple player objects present in scene!");
            }

            Instance = this;
            curDashCooldownLeft = 0;
            curDashFreezeLeft = 0;
            // dashCooldownMeter.fillAmount = 0;
        
            if (levelXpRequirements.Count != maxLevel + 1) Debug.LogError("LevelXpReqs array length not matching!");
            if (speedModifiers.Count != maxLevel + 1) Debug.LogError("speedModifiers array length not matching!");
            if (damageModifiers.Count != maxLevel + 1) Debug.LogError("damageModifiers array length not matching!");
            if (rangeModifiers.Count != maxLevel + 1) Debug.LogError("rangeModifiers array length not matching!");
            if (cooldownModifiers.Count != maxLevel + 1) Debug.LogError("cooldownModifiers array length not matching!");
            if (knockbackModifiers.Count != maxLevel + 1) Debug.LogError("knockbackModifiers array length not matching!");
            if (transformSizeModifiers.Count != maxLevel + 1) Debug.LogError("transformSizeModifiers array length not matching!");

            if(animator == null) Debug.LogError("Animator not set!");

            if(attackTrigger == null) Debug.LogError("Light attack trigger not set!");
            attackTrigger.RegisterOnHit(AttackHit);
            _attackTriggerGameObject = attackTrigger.gameObject;
            _attackTriggerGameObject.SetActive(false);
            CurrentAttackState = AttackState.NotAttacking;
        
            CurLevel = 0;
            curXp = 0;
            UpdateXpMeter();
        }

        void Start()
        {
            _trans = transform;
            _rigidbody = GetComponent<Rigidbody2D>();
            _playerHealth = GetComponent<PlayerHealth>();

            _attackTriggerGameObject.SetActive(false);
            lightAttackLeftSwing.strikeEffectSprite.SetActive(false);
            lightAttackRightSwing.strikeEffectSprite.SetActive(false);
            hasPressedAttackThisCombo = false;
        }

        // Update is called once per frame
        void Update()
        {
            //Do things that are always done first
            UpdateCooldowns();

            if (curDashActiveLeft > 0)
            {
                DashContinue();
                return;
            }
        
            // if (curDashCooldownLeft <= 0 && Input.GetButtonDown("Dash"))
            // { //if we can dash and press dash
            //     if (CurrentAttackState == AttackState.NotAttacking)
            //     {
            //         //just dash normally
            //         DashStart();
            //     }
            //     else if (_curAttack != null && _curAttack.dashCancelable)
            //     {
            //         //cancel attack animation
            //         DashStart();
            //         StopCurrentAttack();
            //     }
            // }

            if (CanMove())
            {
                Move();
            }
        
            //we can try to attack whenever
            // cooldown will be checked inside the function
            if (Input.GetButtonDown("Fire1"))
            {
                if (CurrentAttackState == AttackState.NotAttacking)
                {
                    CurrentAttackComboState = AttackComboState.LeftSwing;
                    _activeAttackCoroutine = Attack(false, AttackComboState.LeftSwing, false);
                    StartCoroutine(_activeAttackCoroutine);
                }
                else if(CurrentAttackState == AttackState.WinddownReady && !hasPressedAttackThisCombo)
                {
                    StopCurrentAttack();
                    CurrentAttackState = AttackState.ActiveAttack;
                    ContinueAttackCombo();
                }
                else if(CurrentAttackState == AttackState.WinddownPre && !hasPressedAttackThisCombo)
                {
                    hasPressedAttackThisCombo = true;
                }
            }
        }

        private void UpdateCooldowns()
        {
            if (curDashCooldownLeft >= 0)
            {
                curDashCooldownLeft -= Time.deltaTime;

                float dashMeterFill = (curDashCooldownLeft > 0) ? curDashCooldownLeft / dashCooldownTime : 0;

                // dashCooldownMeter.fillAmount = dashMeterFill;
            }
        
            if(curDashFreezeLeft > 0)
            { //post dash freeze
                curDashFreezeLeft -= Time.deltaTime;
            
                //TODO: inefficient, and we make this object unmovable
                _rigidbody.velocity = Vector2.zero;
                animator.SetBool("isMoving", false);
            }
        }

        public bool CanMove()
        {
            if (curDashFreezeLeft > 0) return false;

            if (CurrentAttackState == AttackState.ActiveAttack && _curAttack.strikeRestriction == AttackMovementRestriction.Stop)
                return false;

            if (CurrentAttackState == AttackState.Windup && _curAttack.anticipationRestriction == AttackMovementRestriction.Stop)
                return false;


            if (CurrentAttackState is AttackState.WinddownPre or AttackState.WinddownReady &&
                _curAttack.recoveryRestriction == AttackMovementRestriction.Stop)
                return false;
        
            return true;
        }

        private void Move()
        {
            //get basic input dir
            float inputH = Input.GetAxisRaw("Horizontal");
            float inputV = Input.GetAxisRaw("Vertical");

            if (inputH == 0 && inputV == 0)
                plusRotValue = 0;
            else
                plusRotValue = 90;
        
        
            //rotation
            // make it so look rot stays
            if (inputH != 0 || inputV != 0)
            {
                if (Math.Abs(inputH - lastInputH) > _floatingTolerance ||
                    Math.Abs(inputV - lastInputV) > _floatingTolerance)
                {
                    if (inputH == 0 && inputV == 0)
                        plusRotValue = 0;
                    else
                        plusRotValue = 90;

                    float lookH = inputH;
                    float lookV = inputV;
                
                    //restrict diagonal
                    if (Mathf.Abs(inputH) > 0 && Mathf.Abs(inputV) > 0)
                    {
                        //dont rotate
                        lookH = _last4WayDir.x;
                        lookV = _last4WayDir.y;
                    }
                    else
                    {
                        _last4WayDir.x = lookH;
                        _last4WayDir.y = lookV;
                    }
                
                    float rotZ = Mathf.Atan2(lookV, lookH) * Mathf.Rad2Deg;
                
                    float finalRot = rotZ - plusRotValue;
                    _trans.rotation = Quaternion.Euler(0, 0, finalRot);
                    rotatedThisUpdate = true;

                    lastInputH = inputH;
                    lastInputV = inputV;
                
                    SetMecanimRotation(lookH, lookV);
                }
            
                //move if there is input
                float finalSpeed = baseSpeed * speedModifiers[CurLevel];
                if ((CurrentAttackState == AttackState.Windup &&
                     _curAttack.anticipationRestriction == AttackMovementRestriction.HalfSpeed)
                    || (CurrentAttackState == AttackState.Windup &&
                        _curAttack.anticipationRestriction == AttackMovementRestriction.HalfSpeed)
                    || (CurrentAttackState == AttackState.ActiveAttack &&
                        _curAttack.strikeRestriction == AttackMovementRestriction.HalfSpeed))
                {
                    finalSpeed /= 2;
                }
            
                Vector2 newFullForce = new Vector2(inputH, inputV) * (finalSpeed * Time.deltaTime);
                _rigidbody.AddForce(newFullForce);
            
                //this is bogus calculation, doesnt change much
                _rigidbody.velocity = Vector2.ClampMagnitude(_rigidbody.velocity, finalSpeed);
            
                animator.SetBool("isMoving", true);
            }
            else
            {
                //reset movement if no input
                _rigidbody.velocity = Vector2.zero;
                animator.SetBool("isMoving", false);
            }
        }

        private void SetMecanimRotation(float inputH, float inputV)
        {
            float absH = Mathf.Abs(inputH);
            float absV = Mathf.Abs(inputV);

            Vector2 finalVec = Vector2.zero;

            if (absH > absV)
            {
                finalVec = inputH > 0 ? Vector2.right : Vector2.left;
            }
            else if (absH < absV)
            {
                finalVec = inputV > 0 ? Vector2.up : Vector2.down;
            }
        
            animator.SetFloat("lookH", finalVec.x);
            animator.SetFloat("lookV", finalVec.y);
        }

        void DashStart()
        {
            curDashActiveLeft = dashActiveTime;
            curDashCooldownLeft = dashCooldownTime;
            // dashCooldownMeter.fillAmount = 1;

            curDashFreezeLeft = dashPostFreezeTime; //will only start decreasing after curDashActiveLeft

            AudioManager.Instance.PlayAudio(dashAudio);

            DashMove();
        }

        void DashContinue()
        {
            DashMove();
        
            curDashActiveLeft -= Time.deltaTime;

            if (curDashActiveLeft <= 0)
            {
                //implement dash over effects here
            }
        }

        void DashMove()
        {
            Vector2 dashDir = Vector2.zero;
        
            switch (dashType)
            {
                case DashType.DashForward:
                    dashDir.x = lastInputH;
                    dashDir.y = lastInputV;
                    break;
                case DashType.QuickstepBack:
                    dashDir.x = -lastInputH;
                    dashDir.y = -lastInputV;
                    break;
                default:
                    Debug.LogWarning("Bruh this shouldnt happen");
                    break;
            }

            //clamp cur vel + new force
            Vector2 newFullForce = dashDir * (dashSpeed * speedModifiers[CurLevel] * Time.deltaTime);
            _rigidbody.AddForce(newFullForce);
            _rigidbody.velocity = Vector2.ClampMagnitude(_rigidbody.velocity, dashSpeed * speedModifiers[CurLevel]);

            // _trans.Translate(dashDir * (dashSpeed * speedModifier * Time.deltaTime), Space.World);
        }

        private void LateUpdate()
        {
            if (rotatedThisUpdate)
            {
                //player sprite shouldnt rotate
                playerSprite.rotation = Quaternion.Euler(0, 0, _trans.rotation.z * -1.0f);
            
                rotatedThisUpdate = false;
            }
        }

        IEnumerator Attack(bool skipWindup, AttackComboState newComboState, bool chainedAttack = false)
        {
            switch (newComboState)
            {
                case AttackComboState.LeftSwing:
                    animator.SetFloat("AttackCombo", 0);
                    _curAttack = lightAttackLeftSwing;
                    break;
                case AttackComboState.RightSwing:
                    animator.SetFloat("AttackCombo", 1);
                    _curAttack = lightAttackRightSwing;
                    break;
                default:
                    Debug.LogError("Brother you forgot to implement this attack properly");
                    break;
            }
            
            _curAttack.strikeEffectSprite.SetActive(true);
            hasPressedAttackThisCombo = false;
            
            AudioManager.Instance.PlayAudio(_curAttack.strikeAudio);
         
            //TODO: should add listeners so it isnt calculated here every time
            RecalculateAttackAnimSpeeds();


            //##############################################################################################################
            if (!skipWindup)
            {
                CurrentAttackState = AttackState.Windup;
                animator.SetTrigger("Attack");
            
                if (_curAttack.anticipationRestriction is AttackMovementRestriction.Stop or AttackMovementRestriction.HalfSpeed)
                    _rigidbody.velocity = Vector2.zero;

                yield return new WaitForSeconds(_curAttack.anticipationTime * cooldownModifiers[CurLevel]);
            }
            else
            {
                animator.SetTrigger("AttackSkipWindup");
            }


            //##############################################################################################################
            CurrentAttackState = AttackState.ActiveAttack;
            if(_curAttack.anticipationRestriction is AttackMovementRestriction.Stop or AttackMovementRestriction.HalfSpeed)
                _rigidbody.velocity = Vector2.zero;
        
            //enables the triggers, calls OnEnable() on it
            _attackTriggerGameObject.SetActive(true);
            _curAttack.strikeEffectSprite.SetActive(true);
            yield return new WaitForSeconds(_curAttack.strikeTime * cooldownModifiers[CurLevel]);

            //##############################################################################################################
            CurrentAttackState = AttackState.WinddownPre;
            if(_curAttack.recoveryRestriction is AttackMovementRestriction.Stop or AttackMovementRestriction.HalfSpeed)
                _rigidbody.velocity = Vector2.zero;
        
            _attackTriggerGameObject.SetActive(false);
            _curAttack.strikeEffectSprite.SetActive(false);
            yield return new WaitForSeconds(_curAttack.recoveryTime * cooldownModifiers[CurLevel]);
        
        
            //##############################################################################################################
            if (hasPressedAttackThisCombo)
            {
                _activeAttackCoroutine = null;
                //TODO: may have to wait until mecanim steps into new state?

                StopCurrentAttack();
                CurrentAttackState = AttackState.ActiveAttack;
                ContinueAttackCombo();
                yield break;
            }
        
            CurrentAttackState = AttackState.WinddownReady;
        
            yield return new WaitForSeconds(_curAttack.recoveryReadiedTime * cooldownModifiers[CurLevel]);

            //##############################################################################################################
            CurrentAttackState = AttackState.Cooldown;
        
            yield return new WaitForSeconds(_curAttack.cooldownTime * cooldownModifiers[CurLevel]);

            //##############################################################################################################
            CurrentAttackState = AttackState.NotAttacking;
        }

        public void ContinueAttackCombo()
        {
            CurrentAttackComboState = (CurrentAttackComboState == AttackComboState.LeftSwing)
                ? AttackComboState.RightSwing
                : AttackComboState.LeftSwing;
            _activeAttackCoroutine = Attack(true, CurrentAttackComboState, true);
            StartCoroutine(_activeAttackCoroutine);
        }

        public void StopCurrentAttack()
        {
            if(_activeAttackCoroutine != null)
                StopCoroutine(_activeAttackCoroutine);
        
            CurrentAttackState = AttackState.NotAttacking;
        
            //make sure we disable everything, regardless of actual state
            _attackTriggerGameObject.SetActive(false);
            _curAttack?.strikeEffectSprite.SetActive(false);
        
            //reset mecanim triggers too
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("AttackSkipWindup");
        }

        private void RecalculateAttackAnimSpeeds()
        {
            if(_curAttack == null) return;
            
            animator.SetFloat("AttackWindupTime", 
                _curAttack.anticipationReferenceAnim.length / (_curAttack.anticipationTime * cooldownModifiers[CurLevel]));
        
            animator.SetFloat("AttackMainTime", 
                _curAttack.strikeReferenceAnim.length / (_curAttack.strikeTime * cooldownModifiers[CurLevel]));
        
            animator.SetFloat("AttackWinddownTime", 
                _curAttack.recoveryReferenceAnim.length / (_curAttack.recoveryTime * cooldownModifiers[CurLevel]));
        
            animator.SetFloat("AttackWinddownComboReadyTime", 
                _curAttack.recoveryReadiedReferenceAnim.length / (_curAttack.recoveryReadiedTime * cooldownModifiers[CurLevel]));
        }

        public void AttackHit(EnemyHealth enemy)
        {
            Vector2 dirToTarg = enemy.transform.position - _trans.position;
            enemy.Damage(Mathf.FloorToInt(_curAttack.damage * damageModifiers[CurLevel]));
            enemy.Knockback(_curAttack.knockoutTime * knockbackModifiers[CurLevel],
                dirToTarg * (_curAttack.knockoutForce * knockbackModifiers[CurLevel]));
        }

        /// <summary>
        /// Called from PlayerHealth
        /// </summary>
        public void PlayerGetDamage(Vector2 knockBackVector)
        {
            StopCurrentAttack();
        
            _rigidbody.AddForce(knockBackVector * baseKnockBackedForce);
        }

        public bool IsDashing()
        {
            return curDashActiveLeft > 0;
        }

        public void AddXp(int xp)
        {
            curXp += xp;
            UpdateXpMeter();
        
            if (curXp >= levelXpRequirements[CurLevel]) Grow();
        
        }

        public void Grow(bool negative = false)
        {
            if (CurLevel >= maxLevel && !negative)
            {
                Debug.Log("....you won?");
                return;
            }

            if (CurLevel - 1 < 0 && negative)
            {
                Debug.LogWarning("Trying to degrow into negative levels!");
                return;
            }

            CurLevel = (negative) ? CurLevel - 1 : CurLevel + 1;

            AudioManager.Instance.PlayAudio(growAudio);

            curXp = 0;

            float newTransformScale = transformSizeModifiers[CurLevel];
            transform.localScale = new Vector3(newTransformScale, newTransformScale, newTransformScale);

            //TODO: this should be removed, right...?
            // float newHitboxScale = attackRanges * rangeModifiers[CurLevel];
            // _lightAttackTriggerGameObject.transform.localScale =
            //     new Vector3(newHitboxScale, newHitboxScale , newHitboxScale);

            if (CurLevel == size2Level && !negative)
            {
                animator.SetFloat("Size", 2);
            }

            if (CurLevel == size2Level - 1 && negative)
            {
                animator.SetFloat("Size", 1);
            }
        
            UpdateXpMeter();
            _playerHealth.Grow();
        
            //there might be no enemy spawners present (debug room)
            if(EnemySpawner.Instance != null)
                EnemySpawner.Instance.Grow();
        }

        private void UpdateXpMeter()
        {
            xpMeter.fillAmount = (curXp * 1.0f) / (levelXpRequirements[CurLevel] * 1.0f);
            int levelToDisplay = CurLevel + 1;
            levelDisplayText.text = "Size " + levelToDisplay;
        }
    
    }
}
