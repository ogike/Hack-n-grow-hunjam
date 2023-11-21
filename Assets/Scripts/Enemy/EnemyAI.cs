using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyAttackState { NotAttacking, Windup, ActiveAttack, Wincdown, Cooldown }

public enum EnemyState { Moving, Attacking, Knockback }

public class EnemyAI : MonoBehaviour
{
    private EnemyState _state;
    private EnemyAttackState _attackState; 
    
    public float speed = 5;

    [Header("Attacking")]
    public float attackRange;
    public int attackDamage;
    public float attackCooldown;

    private float curAttackCooldown;
    private float curKnockoutTime;

    
    [Header("Sounds")]
    public AudioClip attackAudio;
    
    [Header("References")]
    // public AttackHitbox attackHitbox;
    public Animator animator;
    public GameObject attackEffect;

    //private refs, state vars
    private Transform _playerTrans;
    private Rigidbody2D _myRigid;
    private PlayerHealth _playerHealth;
    private GameManager _gameManager;
    
    //Update() cached variables
    private Vector2 dirToPlayer;
    private float distanceToPlayer;
    
    void Start()
    {
        _playerTrans = PlayerController.Instance.transform;
        _playerHealth = _playerTrans.GetComponent<PlayerHealth>();
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

        //this is a hack - has to be replaced by triggers
        attackRange *= PlayerController.Instance.transform.localScale.x;

        curKnockoutTime = 0;
        _state = EnemyState.Moving;

        attackEffect.SetActive(false);
    }

    void Update()
    {
        HandleCooldowns();
        
        // Dont do anything when knocked back
        if(_state == EnemyState.Knockback) return;
        
        
        Vector2 myPos = transform.position;
        Vector2 playerPos = _playerTrans.position;
        
        dirToPlayer = playerPos - myPos;
        distanceToPlayer = dirToPlayer.magnitude;
        dirToPlayer.Normalize();
        
        //Attack player
        if (_state == EnemyState.Moving)
        {
            if (distanceToPlayer < attackRange && _gameManager.EnemyAttackEnabled)
            {
                Attack(dirToPlayer);
            }
            else if (_gameManager.EnemyMovementEnabled) //Move towards player
            {
                _myRigid.AddForce(dirToPlayer * (speed * Time.deltaTime));
                animator.SetFloat("dirH", dirToPlayer.x);
            }
        }

    }

    void Attack(Vector2 dir)
    {
        _state = EnemyState.Attacking;
        
        attackEffect.SetActive(true);
        curAttackCooldown = attackCooldown;
        
        _playerHealth.TakeDamage(attackDamage);
        
        AudioManager.Instance.PlayAudio(attackAudio);

        //TODO: extra states
        
        //TODO: check if we are in knockback
        //  or would we be interrupted then...?
        _state = EnemyState.Moving;
    }

    void HandleCooldowns()
    {
        if (curKnockoutTime > 0)
        {
            curKnockoutTime -= Time.deltaTime;

            if (curKnockoutTime <= 0)
            {
                if (_state != EnemyState.Knockback)
                {
                    Debug.LogWarning("Knockout time depleted while not in Knockback state, but: " + _state);
                }
                
                //Transition from knockback
                _state = EnemyState.Moving;
            }
        }
        
        if (curAttackCooldown > 0)
        {
            curAttackCooldown -= Time.deltaTime;
            if (curAttackCooldown <= 0)
            {
                attackEffect.SetActive(false);
            }
        }
    }

    public void KnockBack(float knockoutTime)
    {
        _state = EnemyState.Knockback;
        curKnockoutTime += knockoutTime;
       
        //put effects here
    }
}
