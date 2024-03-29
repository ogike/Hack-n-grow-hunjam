using System;
using System.Collections;
using System.Collections.Generic;
using Enemy;
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

public class EnemySpawner : MonoBehaviour
{
    
    public static EnemySpawner Instance { get; private set; }

    public List<LevelEnemySpawnStats> enemySpawnStats;
    private int _curLevel;

    private EnemySpawnerCell[,] _mapCells;
    [FormerlySerializedAs("spawnAreas")] public List<Transform> outsideCameraSpawnAreas;

    public bool debugShowCells;
    
    public float enemyPosZ;

    [Range(0, 100)]
    public int outsideCameraSpawnWeight;
    [Range(0, 100)]
    public int mapCellSpawnWeight;

    public float wanderTimeForCellSpawnedEnemies = 10.0f;

    private float _cellHeight;
    private float _cellWidth;
    
    private float curSpawnWaitTime;

    private Transform _myTrans;
    private PlayerController _playerController;

    public float underMinEnemyCountWaitModifier;
    public float overMaxEnemyCountWaitModifier;

    private int _curEnemyCount = 0;
    private float _curWaitModifier;
    private int _cellRows;
    private int _cellColums;
    private Vector3 _mapBtmLeft;

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
        Vector3 spawnPos;
        float randomNum = Random.Range(0, outsideCameraSpawnWeight + mapCellSpawnWeight);
        bool forceChase;
        if (randomNum < outsideCameraSpawnWeight)
        {
            spawnPos = GetRandomOutsideCameraPosition();
            forceChase = true;
        }
        else
        {
            spawnPos = GetRandomMapCellPosition();
            forceChase = false;
        }

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
        EnemyAI enemyAI = spawnedEnemy.GetComponent<EnemyAI>();
        if(forceChase)  enemyAI.ForceNoWander();
        else            enemyAI.SetSpawnWanderTime(wanderTimeForCellSpawnedEnemies);
    }

    private Vector3 GetRandomOutsideCameraPosition()
    {
        int spawnAreaIndex = Random.Range(0, outsideCameraSpawnAreas.Count);
        Transform spawnArea = outsideCameraSpawnAreas[spawnAreaIndex];

        Vector3 position = spawnArea.position;
        Vector3 lossyScale = spawnArea.lossyScale;
        float minPosX = position.x - (lossyScale.x / 2);
        float maxPosX = position.x + (lossyScale.x / 2);

        float minPosY = position.y - (lossyScale.y / 2);
        float maxPosY = position.y + (lossyScale.y / 2);

        float posX = Random.Range(minPosX, maxPosX);
        float posY = Random.Range(minPosY, maxPosY);

        Vector3 spawnPos = new Vector3(posX, posY, enemyPosZ);
        return spawnPos;
    }

    private Vector3 GetRandomMapCellPosition()
    {
        List<EnemySpawnerCell> validCells = new List<EnemySpawnerCell>();
        Vector2 cameraBtmLeft = CameraFollow.Instance.BottomLeftPos;
        Vector2 cameraTopRight = CameraFollow.Instance.TopRightPos;
        
        for (int col = 0; col < _cellColums; col++)
        {
            for (int row = 0; row < _cellRows; row++)
            {
                if (_mapCells[col, row].IsInCameraView(ref cameraBtmLeft, ref cameraTopRight) == false)
                {
                    validCells.Add(_mapCells[col,row]);
                }
            }
        }

        if (validCells.Count == 0)
        {
            Debug.LogWarning("No valid spawnCell found outside of camera view");
            return GetRandomOutsideCameraPosition();
        }

        int spawnCellIndex = Random.Range(0, validCells.Count);
        return validCells[spawnCellIndex].RandomPositionInside();
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
        _cellRows = Map.Instance.numOfCellRows;
        _cellColums = Map.Instance.numOfCellColumns;
        _cellHeight = Map.Instance.CellHeight;
        _cellWidth = Map.Instance.CellWidth;
        _mapBtmLeft = Map.Instance.MinPosition;
        
        _mapCells = new EnemySpawnerCell[_cellColums,_cellRows];


        for (int row = 0; row < _cellRows; row++)
        {
            for (int col = 0; col < _cellColums; col++)
            {
                Vector2 cellBtmLeft = new Vector2(
                    _mapBtmLeft.x + _cellWidth * col,
                    _mapBtmLeft.y + _cellHeight * row
                );
                
                _mapCells[col, row] = new EnemySpawnerCell(col, row, cellBtmLeft, _cellWidth, _cellHeight);
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
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (_mapCells != null && debugShowCells)
        {
            Vector2 cameraBtmLeft = CameraFollow.Instance.BottomLeftPos;
            Vector2 cameraTopRight = CameraFollow.Instance.TopRightPos;
            Vector3 cellCubeSize = new Vector3(_cellWidth * 0.95f, _cellHeight * 0.95f, 1);
            
            for (int x = 0; x < _cellColums; x++)
            {
                for (int y = 0; y < _cellRows; y++)
                {
                    Vector3 pos = _mapCells[x, y].CenterPos;


                    if (_mapCells[x, y].IsInCameraView(ref cameraBtmLeft, ref cameraTopRight))
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = Color.grey;
                    }

                    Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.2f);

                    Gizmos.DrawCube(pos, cellCubeSize);
                }
            }

            Vector3 cameraSize = new Vector3(
                CameraFollow.Instance.Width,
                CameraFollow.Instance.Height,
                1);
            Vector3 cameraCenter = new Vector3(
                cameraBtmLeft.x + cameraSize.x / 2.0f,
                cameraBtmLeft.y + cameraSize.y / 2.0f, 
                0);
            Gizmos.color =  new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.2f);
            Gizmos.DrawCube(cameraCenter, cameraSize);
        }
    }
#endif //UNITY_EDITOR
}
