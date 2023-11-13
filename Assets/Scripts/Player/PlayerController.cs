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

    public float baseSpeed = 15;

    private float plusRotValue;

    private float lastInputH;
    private float lastInputV;
    private bool rotatedThisUpdate = false;

    private float _floatingTolerance = 0.001f;
    
    //starting level = 0
    [Header("Growth levels")]
    public int maxLevel;

    public int size2Level = 3;
    
    public List<int> levelXpRequirements;
    
    private int curXp;
    public int CurLevel { get; private set; }

    public int curSize;


    public List<float> speedModifiers;
    public List<float> damageModifiers;
    public List<float> rangeModifiers;
    public List<float> cooldownModifiers;
    public List<float> knockbackModifiers;

    public List<float> transformSizeModifiers;

    public AudioClip growAudio;

    //references
    private Transform _trans;
    public Collider2D attackLightTrigger;
    public Transform playerSprite;
    private Rigidbody2D _rigidbody;
    private PlayerHealth _playerHealth;

    //Attack
    public string enemyTag = "Enemy";
    
    public enum AttackState { NotAttacking, Windup, ActiveAttack, Wincdown, Cooldown }
    public AttackState CurrentAttackState { get; private set; }
    private IEnumerator _activeAttackCoroutine;

    [Header("Light attack")]
    public float lightAttackRange;
    public float lightAttackDamage;
    public float lightAttackKnockoutTime;
    public float lightAttackKnockoutForce;
    
    public GameObject attackLightEffect; //enabled when trigger is active

    public float lightAttackCooldown;
    public float lightAttackWindup;
    public float lightAttackTriggerActiveTime;
    public float lightAttackWinddown;
    
    public AudioClip lightAttackAudio;
    public PlayerWeaponTrigger lightAttackTrigger;
    private GameObject _lightAttackTriggerGameObject;

    private Vector2 _last4WayDir;

    [Header("Dashing")] 
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

    [Header("UI")]
    public Image dashCooldownMeter;
    public Image xpMeter;
    public Text levelDisplayText;

    [Header("Animations")] 
    public Animator animator;
    
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
        lightAttackTrigger.Register(LightAttackHit);
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
            DashStart();
        }
        else if(curDashFreezeLeft <= 0)
        { //only move if we are not in dash freeze
            Move();
            
        }
        else
        { //post dash freeze
            curDashFreezeLeft -= Time.deltaTime;
            
            //TODO: inefficient, and we make this object unmovable
            _rigidbody.velocity = Vector2.zero;
            animator.SetBool("isMoving", false);
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
            Vector2 newFullForce = new Vector2(inputH, inputV) * (baseSpeed * speedModifiers[CurLevel] * Time.deltaTime);
            _rigidbody.AddForce(newFullForce);
            _rigidbody.velocity = Vector2.ClampMagnitude(_rigidbody.velocity, baseSpeed * speedModifiers[CurLevel]);
            animator.SetBool("isMoving", true);
        }
        else
        {
            //reset movement if no input
            _rigidbody.velocity = Vector2.zero;
            animator.SetBool("isMoving", false);
        }

        //moving
        // _trans.Translate(new Vector3(inputH, inputV, 0) * (baseSpeed * speedModifier * Time.deltaTime), Space.World);
        // _rigidbody.velocity = new Vector2(inputH, inputV) * (baseSpeed * speedModifier);
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
        if (CurrentAttackState != AttackState.NotAttacking) yield break;
        
        attackLightEffect.SetActive(true);
        
        AudioManager.Instance.PlayAudio(lightAttackAudio);
        animator.SetTrigger("Attack");
        
        CurrentAttackState = AttackState.Windup;
        yield return new WaitForSeconds(lightAttackWindup * cooldownModifiers[CurLevel]);

        CurrentAttackState = AttackState.ActiveAttack;
        
        //enables the triggers, calls OnEnable() on it
        _lightAttackTriggerGameObject.SetActive(true);
        attackLightEffect.SetActive(true);
        yield return new WaitForSeconds(lightAttackTriggerActiveTime * cooldownModifiers[CurLevel]);

        CurrentAttackState = AttackState.Wincdown;
        _lightAttackTriggerGameObject.SetActive(false);
        attackLightEffect.SetActive(false);
        yield return new WaitForSeconds(lightAttackWinddown * cooldownModifiers[CurLevel]);

        CurrentAttackState = AttackState.Cooldown;
        yield return new WaitForSeconds(lightAttackCooldown * cooldownModifiers[CurLevel]);

        CurrentAttackState = AttackState.NotAttacking;
        yield break;
    }

    public void StopLightAttack()
    {
        CurrentAttackState = AttackState.NotAttacking;
        
        //make sure we disable everything, regardless of actual state
        _lightAttackTriggerGameObject.SetActive(false);
        attackLightEffect.SetActive(false);
    }

    public void LightAttackHit(EnemyHealth enemy)
    {
        Vector2 dirToTarg = enemy.transform.position - _trans.position;
        enemy.Damage(Mathf.FloorToInt(lightAttackDamage * damageModifiers[CurLevel]));
        enemy.Knockback(lightAttackKnockoutTime * knockbackModifiers[CurLevel],
            dirToTarg * (lightAttackKnockoutForce * knockbackModifiers[CurLevel]));
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

    private void Grow()
    {
        if (CurLevel >= maxLevel)
        {
            Debug.Log("....you won?");
            return;
        }

        CurLevel++;

        AudioManager.Instance.PlayAudio(growAudio);

        curXp = 0;

        float newTransformScale = transformSizeModifiers[CurLevel];
        transform.localScale = new Vector3(newTransformScale, newTransformScale, newTransformScale);

        if (CurLevel == size2Level)
        {
            animator.SetTrigger("Size2Grow");
            animator.SetBool("Size2", true);
        }
        
        UpdateXpMeter();
        _playerHealth.Grow();
        EnemySpawner.Instance.Grow();
    }

    private void UpdateXpMeter()
    {
        xpMeter.fillAmount = (curXp * 1.0f) / (levelXpRequirements[CurLevel] * 1.0f);
        curSize = CurLevel + 1;                
        levelDisplayText.text = "Size " + curSize;
    }
    
}
