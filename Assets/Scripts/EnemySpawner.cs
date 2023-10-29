using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public List<Transform> spawnAreas;
    
    public GameObject enemyGameObject;

    public float enemyPosZ;

    public float minSpawnWaitTime = 1;
    public float maxSpawnWaitTime = 3;
    private float curSpawnWaitTime;

    private Transform _myTrans;

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
            curSpawnWaitTime = Random.Range(minSpawnWaitTime, maxSpawnWaitTime);
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
}
