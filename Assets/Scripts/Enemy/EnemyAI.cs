using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private Transform _playerTrans;
    private Transform _myTrans;
    private Rigidbody2D _myRigid;
    private PlayerHealth _playerHealth;
    private GameManager _gameManager;

    private Vector2 dirToPlayer;
    private float distanceToPlayer;

    public float speed = 5;

    public float attackRange;
    public int attackDamage;
    public float attackCooldown;

    private float curAttackCooldown;

    private float curKnockoutTime;

    public Animator animator;
    
    //sound
    public AudioClip attackAudio;
    
    //effects
    // public AttackHitbox attackHitbox;
    public GameObject attackEffect;

    // Start is called before the first frame update
    void Start()
    {
        _playerTrans = PlayerController.Instance.transform;
        _playerHealth = _playerTrans.GetComponent<PlayerHealth>();
        _gameManager = GameManager.Instance;
        _myTrans = transform;
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

        attackEffect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (curKnockoutTime > 0)
        {
            curKnockoutTime -= Time.deltaTime;
            return; //dont do anything if knocked out
        }
        
        Vector2 myPos = transform.position;
        Vector2 playerPos = _playerTrans.position;
        
        dirToPlayer = playerPos - myPos;
        distanceToPlayer = dirToPlayer.magnitude;
        dirToPlayer.Normalize();
        
        //Attack player
        if (distanceToPlayer < attackRange && curAttackCooldown <= 0 && _gameManager.EnemyAttackEnabled)
        {
            Attack(dirToPlayer);
        }
        else if(_gameManager.EnemyMovementEnabled) //Move towards player
        {
            _myRigid.AddForce(dirToPlayer * (speed * Time.deltaTime));
            animator.SetFloat("dirH", dirToPlayer.x);
        }

        HandleCooldowns();
    }

    void Attack(Vector2 dir)
    {
        attackEffect.SetActive(true);
        curAttackCooldown = attackCooldown;
        
        _playerHealth.TakeDamage(attackDamage);
        
        AudioManager.Instance.PlayAudio(attackAudio);
    }

    void HandleCooldowns()
    {
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
        curKnockoutTime += knockoutTime;
        
        //put effects here
    }
}
