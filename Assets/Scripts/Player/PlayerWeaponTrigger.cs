using System;
using System.Collections;
using System.Collections.Generic;
using Enemy;
using UnityEngine;

public delegate void EnemyHitCallback(EnemyHealth enemy);

[RequireComponent(typeof(Sprite))]
public class PlayerWeaponTrigger : MonoBehaviour
{
    public string enemyTag = "Enemy";
    
    private HashSet<Collider2D> _enemiesHit;
    private EnemyHitCallback onHitCallback;
    private int _startingHashSetCapacity = 10;

    private SpriteRenderer _renderer;

    private void Awake()
    {
        _enemiesHit = new HashSet<Collider2D>(_startingHashSetCapacity);
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        _enemiesHit.Clear();
        
        SetRendererVisibility(GameManager.Instance.ShowAttackTriggers);
        GameManager.Instance.OnShowAttackTriggerChange += SetRendererVisibility;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnShowAttackTriggerChange -= SetRendererVisibility;
    }
    
    public void SetRendererVisibility(bool visible)
    {
        _renderer.enabled = visible;
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(!col.CompareTag(enemyTag)) return;

        //dont hit an enemy twice
        if (_enemiesHit.Contains(col)) return;

        _enemiesHit.Add(col);

        EnemyHealth enemy = col.GetComponent<EnemyHealth>();
        if (enemy == null)
        {
            Debug.LogWarning( col.transform.name + ", with \"Enemy\" tag was hit but doesnt have EnemyHealth component!");
            return;
        }

        onHitCallback(enemy);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //do i even need to do anything
    }
    
    public void RegisterOnHit(EnemyHitCallback callback)
    {
        onHitCallback = callback;
    }
}
