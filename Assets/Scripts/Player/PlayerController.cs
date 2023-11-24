using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


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

    public enum AttackState { NotAttacking, Windup, ActiveAttack, Wincdown, Cooldown }
    public enum AttackMovementRestriction {None, Stop, HalfSpeed}
    
    public AttackState CurrentAttackState { get; private set; }
    private IEnumerator _activeAttackCoroutine;

    [Header("Light attack")] //#########################################################################################
    public float lightAttackRange; //TODO: use for scaling
    public float lightAttackDamage;
    public float lightAttackKnockoutTime;
    public float lightAttackKnockoutForce;
    
    public float lightAttackWindup;
    public float lightAttackTriggerActiveTime;
    public float lightAttackWinddown;
    public float lightAttackCooldown;

    public AttackMovementRestriction lightAttackWindupRestriction;
    public AttackMovementRestriction lightAttackAttackRestriction;
    public AttackMovementRestriction lightAttackWinddownRestriction;
    
    [Tooltip("Can the attack be cancelled by a dash")]
    public bool lightAttackCancelable;
    
    public AudioClip lightAttackAudio;
    public GameObject attackLightEffect; //enabled when trigger is active
    public PlayerWeaponTrigger lightAttackTrigger;
    private GameObject _lightAttackTriggerGameObject;

    private Vector2 _last4WayDir;

    [Header("Dashing")] //##############################################################################################
    public float dashSpeed;
    public float dashActiveTime;
    public float dashCooldownTime;
    public float dashPostFreezeTime;

    public AudioClip dashAudio;

    public enum DashType {DashForward, QuickstepBack}
    public DashType dashType;
    
    private float curDashActiveLeft;
    private float curDashCooldownLeft;
    private float curDashFreezeLeft;

    [Header("UI")] //###################################################################################################
    public Image dashCooldownMeter;
    public Image xpMeter;
    public Text levelDisplayText;

    [Header("Animations")]  //##########################################################################################
    public Animator animator;

    public AnimationClip attackWindUpReferenceAnim;
    public AnimationClip attackMainReferenceAnim;
    public AnimationClip attackWindDownReferenceAnim;
    
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
        dashCooldownMeter.fillAmount = 0;
        
        if (levelXpRequirements.Count != maxLevel + 1) Debug.LogError("LevelXpReqs array length not matching!");
        if (speedModifiers.Count != maxLevel + 1) Debug.LogError("speedModifiers array length not matching!");
        if (damageModifiers.Count != maxLevel + 1) Debug.LogError("damageModifiers array length not matching!");
        if (rangeModifiers.Count != maxLevel + 1) Debug.LogError("rangeModifiers array length not matching!");
        if (cooldownModifiers.Count != maxLevel + 1) Debug.LogError("cooldownModifiers array length not matching!");
        if (knockbackModifiers.Count != maxLevel + 1) Debug.LogError("knockbackModifiers array length not matching!");
        if (transformSizeModifiers.Count != maxLevel + 1) Debug.LogError("transformSizeModifiers array length not matching!");

        if(animator == null) Debug.LogError("Animator not set!");

        if(lightAttackTrigger == null) Debug.LogError("Light attack trigger not set!");
        lightAttackTrigger.RegisterOnHit(LightAttackHit);
        _lightAttackTriggerGameObject = lightAttackTrigger.gameObject;
        _lightAttackTriggerGameObject.SetActive(false);
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

        _lightAttackTriggerGameObject.SetActive(false);
        attackLightEffect.SetActive(false);
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
        
        if (curDashCooldownLeft <= 0 && Input.GetButtonDown("Dash"))
        { //if we can dash and press dash
            if (CurrentAttackState == AttackState.NotAttacking)
            {
                //just dash normally
                DashStart();
            }
            else if (lightAttackCancelable)
            {
                //cancel attack animation
                DashStart();
                StopLightAttack();
            }
        }

        if (CanMove())
        {
            Move();
        }
        
        //we can try to attack whenever
        // cooldown will be checked inside the function
        if (Input.GetButtonDown("Fire1") && CurrentAttackState == AttackState.NotAttacking)
        {
            _activeAttackCoroutine = LightAttack();
            StartCoroutine(_activeAttackCoroutine);
        }
    }

    private void UpdateCooldowns()
    {
        if (curDashCooldownLeft >= 0)
        {
            curDashCooldownLeft -= Time.deltaTime;

            float dashMeterFill = (curDashCooldownLeft > 0) ? curDashCooldownLeft / dashCooldownTime : 0;

            dashCooldownMeter.fillAmount = dashMeterFill;
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

        if (CurrentAttackState == AttackState.ActiveAttack && lightAttackAttackRestriction == AttackMovementRestriction.Stop)
            return false;

        if (CurrentAttackState == AttackState.Windup && lightAttackWindupRestriction == AttackMovementRestriction.Stop)
            return false;


        if (CurrentAttackState == AttackState.Wincdown &&
            lightAttackWinddownRestriction == AttackMovementRestriction.Stop)
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
                
                animator.SetFloat("lookH", inputH);
                animator.SetFloat("lookV", inputV);
            }
            
            //move if there is input
            float finalSpeed = baseSpeed * speedModifiers[CurLevel];
            if ((CurrentAttackState == AttackState.Windup &&
                 lightAttackWindupRestriction == AttackMovementRestriction.HalfSpeed)
                || (CurrentAttackState == AttackState.Windup &&
                    lightAttackWindupRestriction == AttackMovementRestriction.HalfSpeed)
                || (CurrentAttackState == AttackState.ActiveAttack &&
                    lightAttackAttackRestriction == AttackMovementRestriction.HalfSpeed))
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

    void DashStart()
    {
        curDashActiveLeft = dashActiveTime;
        curDashCooldownLeft = dashCooldownTime;
        dashCooldownMeter.fillAmount = 1;

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

    IEnumerator LightAttack()
    {
        attackLightEffect.SetActive(true);
        
        AudioManager.Instance.PlayAudio(lightAttackAudio);
        animator.SetTrigger("Attack");
        
        //TODO: should add listeners so it isnt calculated here every time
        RecalculateAttackAnimSpeeds();
        
        //##############################################################################################################
        CurrentAttackState = AttackState.Windup;
        if(lightAttackWindupRestriction is AttackMovementRestriction.Stop or AttackMovementRestriction.HalfSpeed)
            _rigidbody.velocity = Vector2.zero;
        
        yield return new WaitForSeconds(lightAttackWindup * cooldownModifiers[CurLevel]);

        //##############################################################################################################
        CurrentAttackState = AttackState.ActiveAttack;
        if(lightAttackWindupRestriction is AttackMovementRestriction.Stop or AttackMovementRestriction.HalfSpeed)
            _rigidbody.velocity = Vector2.zero;
        
        //enables the triggers, calls OnEnable() on it
        _lightAttackTriggerGameObject.SetActive(true);
        attackLightEffect.SetActive(true);
        animator.SetTrigger("AttackMain");
        yield return new WaitForSeconds(lightAttackTriggerActiveTime * cooldownModifiers[CurLevel]);

        //##############################################################################################################
        CurrentAttackState = AttackState.Wincdown;
        if(lightAttackWinddownRestriction is AttackMovementRestriction.Stop or AttackMovementRestriction.HalfSpeed)
            _rigidbody.velocity = Vector2.zero;
        
        _lightAttackTriggerGameObject.SetActive(false);
        attackLightEffect.SetActive(false);
        animator.SetTrigger("AttackWinddown");
        yield return new WaitForSeconds(lightAttackWinddown * cooldownModifiers[CurLevel]);

        //##############################################################################################################
        CurrentAttackState = AttackState.Cooldown;
        
        animator.SetTrigger("AttackExit");
        yield return new WaitForSeconds(lightAttackCooldown * cooldownModifiers[CurLevel]);

        //##############################################################################################################
        CurrentAttackState = AttackState.NotAttacking;
        yield break;
    }

    public void StopLightAttack()
    {
        if(_activeAttackCoroutine != null)
            StartCoroutine(_activeAttackCoroutine);
        
        CurrentAttackState = AttackState.NotAttacking;
        
        //make sure we disable everything, regardless of actual state
        _lightAttackTriggerGameObject.SetActive(false);
        attackLightEffect.SetActive(false);
        
        //reset mecanim triggers too
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("AttackMain");
        animator.ResetTrigger("AttackWinddown");
        animator.ResetTrigger("AttackExit");
    }

    private void RecalculateAttackAnimSpeeds()
    {
        animator.SetFloat("AttackWindupTime", 
            attackWindUpReferenceAnim.length / (lightAttackWindup * cooldownModifiers[CurLevel]));
        
        animator.SetFloat("AttackMainTime", 
            attackMainReferenceAnim.length / (lightAttackTriggerActiveTime * cooldownModifiers[CurLevel]));
        
        animator.SetFloat("AttackWinddownTime", 
            attackWindDownReferenceAnim.length / (lightAttackWinddown * cooldownModifiers[CurLevel]));
    }

    public void LightAttackHit(EnemyHealth enemy)
    {
        Vector2 dirToTarg = enemy.transform.position - _trans.position;
        enemy.Damage(Mathf.FloorToInt(lightAttackDamage * damageModifiers[CurLevel]));
        enemy.Knockback(lightAttackKnockoutTime * knockbackModifiers[CurLevel],
            dirToTarg * (lightAttackKnockoutForce * knockbackModifiers[CurLevel]));
    }

    /// <summary>
    /// Called from PlayerHealth
    /// </summary>
    public void PlayerGetDamage(Vector2 knockBackVector)
    {
        StopLightAttack();
        
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

        float newHitboxScale = lightAttackRange * rangeModifiers[CurLevel];
        _lightAttackTriggerGameObject.transform.localScale =
            new Vector3(newHitboxScale, newHitboxScale , newHitboxScale);

        if (CurLevel == size2Level && !negative)
        {
            animator.SetTrigger("Size2Grow");
            animator.SetBool("Size2", true);
        } //implement degrowth

        if (CurLevel == size2Level - 1 && negative)
        {
            animator.SetTrigger("Size1Grow");
            animator.SetBool("Size2", false);
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
        levelDisplayText.text = "Size " + CurLevel + 1;
    }
    
}
