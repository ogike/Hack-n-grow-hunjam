using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private Transform _playerTrans;
    private Transform _myTrans;
    private Rigidbody2D _myRigid;
    private PlayerHealth _playerHealth;

    private Vector2 dirToPlayer;
    private float distanceToPlayer;

    public float speed = 5;

    public float attackRange;
    public int attackDamage;
    public float attackCooldown;

    private float curAttackCooldown;
    
    //effects
    // public AttackHitbox attackHitbox;
    public GameObject attackEffect;

    // Start is called before the first frame update
    void Start()
    {
        _playerTrans = PlayerController.Instance.transform;
        _playerHealth = _playerTrans.GetComponent<PlayerHealth>();
        _myTrans = transform;
        _myRigid = GetComponent<Rigidbody2D>();
        if (_myRigid == null)
        {
            Debug.LogError("No rigidbody2D attached to this enemy!");
        }

        attackEffect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 myPos = transform.position;
        Vector2 playerPos = _playerTrans.position;
        
        dirToPlayer = playerPos - myPos;
        distanceToPlayer = dirToPlayer.magnitude;
        dirToPlayer.Normalize();
        
        //Attack player
        if (distanceToPlayer < attackRange && curAttackCooldown <= 0)
        {
            Attack(dirToPlayer);
        }
        else //Move towards player
        {
            _myRigid.AddForce(dirToPlayer * (speed * Time.deltaTime));
        }

        HandleCooldowns();
    }

    void Attack(Vector2 dir)
    {
        attackEffect.SetActive(true);
        curAttackCooldown = attackCooldown;
        
        _playerHealth.TakeDamage(attackDamage);
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
}
