using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; } 
    
    public List<Transform> spawnAreas;
    
    public GameObject enemyGameObject;

    public float enemyPosZ;

    public float minSpawnWaitTime = 1;
    public float maxSpawnWaitTime = 3;
    private float curSpawnWaitTime;

    private Transform _myTrans;

    public int minEnemyCount = 3;
    public int maxEnemyCount = 6;

    public float underMinEnemyCountWaitModifier;
    public float overMaxEnemyCountWaitModifier;

    private int _curEnemyCount = 0;
    private float _curWaitModifier;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one EnemySpawner!");
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        _myTrans = transform;
        curSpawnWaitTime = 0;
    }

    private void Update()
    {
        if (curSpawnWaitTime < 0)
        {
            SpawnSingleEnemy();
            IncreaseEnemyCount();
            curSpawnWaitTime = Random.Range(
                minSpawnWaitTime * _curWaitModifier,
                maxSpawnWaitTime * _curWaitModifier);
        }
        else
        {
            curSpawnWaitTime -= Time.deltaTime;
        }
    }

    void SpawnSingleEnemy()
    {
        int spawnAreaIndex = Random.Range(0, spawnAreas.Count);
        Transform spawnArea = spawnAreas[spawnAreaIndex];

        float minPosX = spawnArea.position.x - (spawnArea.lossyScale.x / 2);
        float maxPosX = spawnArea.position.x + (spawnArea.lossyScale.x / 2);
        
        float minPosY = spawnArea.position.y - (spawnArea.lossyScale.y / 2);
        float maxPosY = spawnArea.position.y + (spawnArea.lossyScale.y / 2);

        float posX = Random.Range(minPosX, maxPosX);
        float posY = Random.Range(minPosY, maxPosY);

        Vector3 spawnPos = new Vector3(posX, posY, enemyPosZ);

        GameObject spawnedEnemy = GameObject.Instantiate(enemyGameObject, spawnPos, Quaternion.identity, _myTrans);
    }

    public void IncreaseEnemyCount()
    {
        _curEnemyCount++;

        if (_curEnemyCount > maxEnemyCount)
            _curWaitModifier = overMaxEnemyCountWaitModifier;
    }

    public void DecreaseEnemyCount()
    {
        _curEnemyCount--;

        if (_curEnemyCount < minEnemyCount)
            _curWaitModifier = underMinEnemyCountWaitModifier;
    }
}
