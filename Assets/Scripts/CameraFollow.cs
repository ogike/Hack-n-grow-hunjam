using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform _playerTrans;
    private Transform _myTrans;

    public float followSpeed;

    private Vector3 _basePosVelocity;
    private Vector3 targetPos;

    public float maxPosX;
    public float maxPosY;

    
    private void Start()
    {
        _playerTrans = PlayerController.Instance.transform;
        _myTrans = transform;
    }

    void Update()
    {
        Vector3 myPos = _myTrans.position;
        Vector3 playerPos = _playerTrans.position;

        targetPos = playerPos;
        targetPos.z = myPos.z; //set camera z pos separately

        if (targetPos.x > Map.Instance.MaxCameraPosition.x) targetPos.x = Map.Instance.MaxCameraPosition.x;
        else if (targetPos.x < Map.Instance.MinCameraPosition.x) targetPos.x = Map.Instance.MinCameraPosition.x;

        if (targetPos.y > Map.Instance.MaxCameraPosition.y) targetPos.y = Map.Instance.MaxCameraPosition.y;
        else if (targetPos.y < Map.Instance.MinCameraPosition.y) targetPos.y = Map.Instance.MinCameraPosition.y;
        
        _myTrans.position = Vector3.SmoothDamp(myPos, targetPos, ref _basePosVelocity, followSpeed);
    }
}
