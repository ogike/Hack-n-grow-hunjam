using System;
using System.Collections;
using System.Collections.Generic;
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
    
    public List<int> levelXpRequirements;
    
    private int curXp;
    public int CurLevel { get; private set; }
    
    public List<float> speedModifiers;
    public List<float> damageModifiers;
    public List<float> rangeModifiers;
    public List<float> cooldownModifiers;
    public List<float> knockbackModifiers;

    public List<float> transformSizeModifiers;

    //references
    private Transform _trans;
    public Collider2D attackLightTrigger;
    public Transform playerSprite;
    private Rigidbody2D _rigidbody;
    private PlayerHealth _playerHealth;

    //Attack
    public string enemyTag = "Enemy";

    private float curAttackCooldown;
    
    [Header("Light attack")]
    public float lightAttackRange;
    public float lightAttackDamage;
    public float lightAttackKnockoutTime;
    public float lightAttackKnockoutForce;
    
    public GameObject attackLightEffect;
    public float attackLightEffectTime;
    private float attackLightEffectCurTime = 0.5f;

    public float lightAttackCooldown;
    
    public AudioClip lightAttackAudio;

    private Vector2 _last4WayDir;

    [Header("Dashing")] 
    public float dashSpeed;
    public float dashActiveTime;
    public float dashCooldownTime;
    public float dashPostFreezeTime;
    
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
        
        CurLevel = 0;
        curXp = 0;
        UpdateXpMeter();
    }

    void Start()
    {
        _trans = transform;
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerHealth = GetComponent<PlayerHealth>();

        attackLightEffectCurTime = 0;
        attackLightEffect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (curDashActiveLeft <= 0)
        {
            if (curDashCooldownLeft <= 0 && Input.GetButtonDown("Dash"))
            {
                DashStart();
            }
            else if(curDashFreezeLeft <= 0)
            {
                Move();
            }
            else
            { //post dash freeze
                curDashFreezeLeft -= Time.deltaTime;
                _rigidbody.velocity = Vector2.zero;
                animator.SetBool("isMoving", false);
            }
            
            //only be able to attack if not dashing
            if (Input.GetButtonDown("Fire1"))
                LightAttack();
        }
        else
        {
            DashContinue();
        }
        
        //always cool down no matter what
        if (curDashCooldownLeft >= 0)
        {
            curDashCooldownLeft -= Time.deltaTime;

            float dashMeterFill = (curDashCooldownLeft > 0) ? curDashCooldownLeft / dashCooldownTime : 0;

            dashCooldownMeter.fillAmount = dashMeterFill;
        }

        UpdateEffect();
        
        if (curAttackCooldown >= 0)
        {
            curAttackCooldown -= Time.deltaTime;
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

    void LightAttack()
    {
        if (curAttackCooldown > 0)
        {
            return;
        }

        curAttackCooldown += lightAttackCooldown * cooldownModifiers[CurLevel];
        
        Debug.DrawLine(_trans.position, _trans.position + _trans.up * (lightAttackRange * rangeModifiers[CurLevel]), 
                        Color.yellow, attackLightEffectTime);
        Debug.DrawLine(_trans.position, _trans.position + _trans.right * (lightAttackRange * rangeModifiers[CurLevel]), 
                        Color.yellow, attackLightEffectTime);
        Debug.DrawLine(_trans.position, _trans.position - _trans.right * (lightAttackRange * rangeModifiers[CurLevel]), 
                        Color.yellow, attackLightEffectTime);
        
        attackLightEffect.SetActive(true);
        attackLightEffectCurTime = attackLightEffectTime;

        Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position, lightAttackRange * rangeModifiers[CurLevel]);
        if (colls.Length == 0)
        {
            return;
        }

        Vector3 curPos = _trans.position;
        Vector2 forwardDir = _trans.up;

        AudioManager.Instance.PlayAudio(lightAttackAudio);

        for (int i = 0; i < colls.Length; i++)
        {
            if(!colls[i].CompareTag(enemyTag)) continue;

            Vector2 dirToTarg = colls[i].transform.position - curPos;
            float angle = Vector2.Angle(forwardDir, dirToTarg);
            
            if (angle < 100) //succesful hit
            {
                Debug.DrawLine(curPos, curPos + (Vector3)dirToTarg, Color.red, attackLightEffectTime);
                EnemyHealth enemyHealth = colls[i].GetComponent<EnemyHealth>();
                if (enemyHealth == null)
                {
                    Debug.LogError("No enemyhealth attached to GameObject with Enemy tag!");
                    continue;
                }
                
                enemyHealth.Damage(Mathf.FloorToInt(lightAttackDamage * damageModifiers[CurLevel]));
                enemyHealth.Knockback(lightAttackKnockoutTime * knockbackModifiers[CurLevel],
                    dirToTarg * (lightAttackKnockoutForce * knockbackModifiers[CurLevel]));
            }
            else //behind player
            {
                Debug.DrawLine(curPos, curPos + (Vector3)dirToTarg, Color.black, attackLightEffectTime);
            }

        }
    }

    void UpdateEffect()
    {
        if (attackLightEffectCurTime > 0)
        {
            attackLightEffectCurTime -= Time.deltaTime;

            if (attackLightEffectCurTime <= 0)
            {
                attackLightEffect.SetActive(false);
            }
        }
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
        curXp = 0;

        float newTransformScale = transformSizeModifiers[CurLevel];
        transform.localScale = new Vector3(newTransformScale, newTransformScale, newTransformScale);
        
        UpdateXpMeter();
       _playerHealth.Grow();
    }

    private void UpdateXpMeter()
    {
        xpMeter.fillAmount = (curXp * 1.0f) / (levelXpRequirements[CurLevel] * 1.0f);
        levelDisplayText.text = "Level " + CurLevel;
    }
    
}
