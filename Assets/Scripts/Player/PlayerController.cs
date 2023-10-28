using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    
    public float baseSpeed = 15;
    public float speedModifier = 1;

    private float plusRotValue;

    private float lastInputH;
    private float lastInputV;
    private bool rotatedThisUpdate = false;

    private float _floatingTolerance = 0.001f;
    
    //references
    private Transform _trans;
    public Collider2D attackLightTrigger;
    public Transform playerSprite;

    //Attack
    public string enemyTag = "Enemy";
    public float damageModifier = 1;
    public float rangeModifier = 1;

    private float curAttackCooldown;
    
    //Light attack
    public float lightAttackRange;
    public float lightAttackDamage;
    
    public GameObject attackLightEffect;
    public float attackLightEffectTime;
    private float attackLightEffectCurTime = 0.5f;

    public float lightAttackCooldown;

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

    public Image dashCooldownMeter;

    public AudioClip lightAttackAudio;

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
    }

    void Start()
    {
        _trans = transform;

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
            {
                curDashFreezeLeft -= Time.deltaTime;
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
                    //dont rotatedd
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
            }
        }
        
        //moving
        _trans.Translate(new Vector3(inputH, inputV, 0) * (baseSpeed * speedModifier * Time.deltaTime), Space.World);
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
        Vector3 dashDir = Vector3.zero;
        
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
        
        _trans.Translate(dashDir * (dashSpeed * speedModifier * Time.deltaTime), Space.World);

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

        curAttackCooldown += lightAttackCooldown;
        
        Debug.DrawLine(_trans.position, _trans.position + _trans.up * (lightAttackRange * rangeModifier), 
                        Color.yellow, attackLightEffectTime);
        Debug.DrawLine(_trans.position, _trans.position + _trans.right * (lightAttackRange * rangeModifier), 
                        Color.yellow, attackLightEffectTime);
        Debug.DrawLine(_trans.position, _trans.position - _trans.right * (lightAttackRange * rangeModifier), 
                        Color.yellow, attackLightEffectTime);
        
        attackLightEffect.SetActive(true);
        attackLightEffectCurTime = attackLightEffectTime;

        Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position, lightAttackRange * rangeModifier);
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
                
                enemyHealth.Damage(Mathf.FloorToInt(lightAttackDamage * damageModifier));
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
    
}
