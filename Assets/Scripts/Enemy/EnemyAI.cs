using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
    public float attackKnockBackAmount = 1;

    public float attackWindupTime;
    public float attackActiveTime;
    public float attackWinddownTime;
    public float attackCooldownTime;

    private IEnumerator _attackCoroutine;
    
    //TODO: should not exist
    private float curAttackCooldown;
    private float curKnockoutTime;

    
    [Header("Sounds")]
    public AudioClip attackAudio;
    
    [Header("References")]
    public EnemyAttackTrigger attackHitbox;
    public Animator animator;
    public GameObject attackEffect;

    //private refs, state vars
    private Transform _playerTrans;
    private Rigidbody2D _myRigid;
    private PlayerHealth _playerHealth;
    private GameManager _gameManager;
    private GameObject _attackHitboxGameObject;
    
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

        if (attackHitbox == null)
        {
            Debug.LogError("No attack hitbox/trigger attached to this enemy!");
        }
        attackHitbox.RegisterOnHit(AttackOnHit);
        _attackHitboxGameObject = attackHitbox.gameObject;
        _attackHitboxGameObject.SetActive(false);
        
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
                _attackCoroutine = Attack();
                StartCoroutine(_attackCoroutine);
            }
            else if (_gameManager.EnemyMovementEnabled) //Move towards player
            {
                _myRigid.AddForce(dirToPlayer * (speed * Time.deltaTime));
                animator.SetFloat("dirH", dirToPlayer.x);
            }
        }

    }

    IEnumerator Attack()
    {
        _state = EnemyState.Attacking;
        
        //Charging up
        _attackState = EnemyAttackState.Windup;
        animator.SetTrigger("AttackState");
        attackEffect.SetActive(true);

        yield return new WaitForSeconds(attackWindupTime);
        
        //Active attack
        _attackState = EnemyAttackState.ActiveAttack;
        _attackHitboxGameObject.SetActive(true);
        
        animator.SetTrigger("AttackMain");
        AudioManager.Instance.PlayAudio(attackAudio);
        attackEffect.SetActive(false);
        
        yield return new WaitForSeconds(attackActiveTime);

        _attackState = EnemyAttackState.Wincdown;
        _attackHitboxGameObject.SetActive(false);

        animator.SetTrigger("AttackWinddown");

        
        yield return new WaitForSeconds(attackWinddownTime);

        _attackState = EnemyAttackState.Cooldown;
        //TODO: make sure we can move in this state

        animator.SetTrigger("AttackExit");

        yield return new WaitForSeconds(attackCooldownTime);

        
        //TODO: check if we are in knockback
        //  or would we be interrupted then...?
        _state = EnemyState.Moving;
        _attackState = EnemyAttackState.NotAttacking;
    }

    /// <summary>
    /// Called by the EnemyAttackTrigger hitbox component on collision
    /// </summary>
    public void AttackOnHit(PlayerHealth playerHealth)
    {
        //TODO: attack cancelling
        Debug.Log("Hitting player with: " + attackDamage);
        playerHealth.TakeDamage(attackDamage, dirToPlayer * attackKnockBackAmount);
        
        //effects go here
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
