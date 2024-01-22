using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
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

[Serializable]
public class EnemySpawnerCell
{
    public int xIndex { get; private set; }
    public int yIndex { get; private set; }

    public Vector2 btmLeftPos;

    public float width;
    public float height;

    public EnemySpawnerCell(int x, int y, Vector2 btmLeft, float _width, float _height)
    {
        xIndex = x;
        yIndex = y;
        btmLeftPos = btmLeft;
        width = _width;
        height = _height;
    }

    public Vector2 RandomPositionInside()
    {
        return new Vector2(
            Random.Range(btmLeftPos.x, btmLeftPos.x + width),
            Random.Range(btmLeftPos.y, btmLeftPos.y + height)
        );
    }
    
}

public class EnemySpawner : MonoBehaviour
{
    
    public static EnemySpawner Instance { get; private set; }

    public List<LevelEnemySpawnStats> enemySpawnStats;
    private int _curLevel;

    private EnemySpawnerCell[,] _mapCells;
    [FormerlySerializedAs("spawnAreas")] public List<Transform> outsideCameraSpawnAreas;
    public float enemyPosZ;

    public float outsideCameraSpawnWeight;
    public float mapCellSpawnWeight;

    private float _cellHeight;
    private float _cellWidth;
    
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
        InitMapCells();
        

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
        Vector3 spawnPos = GetRandomSpawnPosition();

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

    private Vector3 GetRandomSpawnPosition()
    {
        
    }

    private Vector3 GetRandomOutsideCameraPosition()
    {
        int spawnAreaIndex = Random.Range(0, outsideCameraSpawnAreas.Count);
        Transform spawnArea = outsideCameraSpawnAreas[spawnAreaIndex];

        float minPosX = spawnArea.position.x - (spawnArea.lossyScale.x / 2);
        float maxPosX = spawnArea.position.x + (spawnArea.lossyScale.x / 2);

        float minPosY = spawnArea.position.y - (spawnArea.lossyScale.y / 2);
        float maxPosY = spawnArea.position.y + (spawnArea.lossyScale.y / 2);

        float posX = Random.Range(minPosX, maxPosX);
        float posY = Random.Range(minPosY, maxPosY);

        Vector3 spawnPos = new Vector3(posX, posY, enemyPosZ);
        return spawnPos;
    }

    private Vector3 GetRandomMapCellPosition()
    {
        Vector2 cameraBtmLeftPos = 
    }

    /// <summary>
    /// Used by debug menu
    /// </summary>
    public static void SpawnSingleDefaultEnemy(Vector3 position, GameObject enemy)
    {
        if (enemy == null)
        {
            Debug.LogError("No enemy given to spawn in debug menu!");
        }

        GameObject spawnedEnemy = GameObject.Instantiate(enemy, position, Quaternion.identity);
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

    public void InitMapCells()
    {
        int rows = Map.Instance.numOfCellRows;
        int columns = Map.Instance.numOfCellColumns;
        _cellHeight = Map.Instance.CellWidth;
        _cellWidth = Map.Instance.CellHeight;
        Vector3 mapBtmLeft = Map.Instance.MinPosition;
        
        _mapCells = new EnemySpawnerCell[rows,columns];


        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector2 cellBtmLeft = new Vector2(
                    mapBtmLeft.x + _cellWidth * row,
                    mapBtmLeft.y + _cellHeight * col
                );
                _mapCells[row, col] = new EnemySpawnerCell(row, col, cellBtmLeft, _cellWidth, _cellHeight);
            }
        }
    }

    /// <summary>
    /// Resets stats according to CurLevel
    /// </summary>
    public void Grow()
    {
        _curLevel = _playerController.CurLevel;
    }
}
