using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class EnemySpawnWeight
{
    public GameObject enemy;
    public int spawnWeight = 1;
}

[Serializable]
public class LevelEnemySpawnStats
{
    public List<EnemySpawnWeight> spawnWeights;
    public int minEnemyCount = 3;
    public int maxEnemyCount = 6;
    public float minSpawnWaitTime = 1;
    public float maxSpawnWaitTime = 3;
}

public class EnemySpawner : MonoBehaviour
{
    
    public static EnemySpawner Instance { get; private set; }

    public List<LevelEnemySpawnStats> enemySpawnStats;
    private int _curLevel;

    public List<Transform> spawnAreas;
    public float enemyPosZ;
    
    private float curSpawnWaitTime;

    private Transform _myTrans;
    private PlayerController _playerController;

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
        _playerController = PlayerController.Instance;
        _curLevel = _playerController.CurLevel;

        curSpawnWaitTime = 0;
    }

    private void Update()
    {
        if (curSpawnWaitTime < 0)
        {
            SpawnSingleEnemy();
            IncreaseEnemyCount();
            curSpawnWaitTime = Random.Range(
                enemySpawnStats[_curLevel].minSpawnWaitTime * _curWaitModifier,
                enemySpawnStats[_curLevel].maxSpawnWaitTime * _curWaitModifier);
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

        int maxWeight = 0;
        foreach (var weightStat in enemySpawnStats[_curLevel].spawnWeights)
        {
            maxWeight += weightStat.spawnWeight;
        }

        if (maxWeight == 0)
        {
            Debug.LogError("Sum of spawn weight is 0 in level " + _curLevel);
        }

        int chosenWeightNum = Random.Range(0, maxWeight+1);

        GameObject enemyToSpawn = null;
        int curWeight = 0;
        foreach (var weightStat in enemySpawnStats[_curLevel].spawnWeights)
        {
            if (chosenWeightNum <= curWeight + weightStat.spawnWeight)
            {
                enemyToSpawn = weightStat.enemy;
                break;
            }
            else
            {
                curWeight += weightStat.spawnWeight;
            }
        }

        if (enemyToSpawn == null)
        {
            Debug.LogError("ran out of weightStats while looking for chosen enemy weight!");
            return;
        }

        GameObject spawnedEnemy = GameObject.Instantiate(enemyToSpawn, spawnPos, Quaternion.identity, _myTrans);
    }

    public void IncreaseEnemyCount()
    {
        _curEnemyCount++;

        if (_curEnemyCount > enemySpawnStats[_curLevel].maxEnemyCount)
            _curWaitModifier = overMaxEnemyCountWaitModifier;

        if (_curEnemyCount < enemySpawnStats[_curLevel].maxEnemyCount && _curEnemyCount > enemySpawnStats[_curLevel].minEnemyCount)
            _curWaitModifier = 1;
    }

    public void DecreaseEnemyCount()
    {
        _curEnemyCount--;

        if (_curEnemyCount < enemySpawnStats[_curLevel].minEnemyCount)
            _curWaitModifier = underMinEnemyCountWaitModifier;
        
        if (_curEnemyCount < enemySpawnStats[_curLevel].maxEnemyCount && _curEnemyCount > enemySpawnStats[_curLevel].minEnemyCount)
            _curWaitModifier = 1;
    }

    public void Grow()
    {
        _curLevel = _playerController.CurLevel;
    }
}
