using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }
    public Camera Camera { get; private set; }
    
    private Transform _playerTrans;
    private Transform _myTrans;

    public float followSpeed;

    private Vector3 _basePosVelocity;
    private Vector3 targetPos;

    public float maxPosX;
    public float maxPosY;
    
    public float Height => Camera.orthographicSize * 2;
    public float Width => Camera.aspect * Height;

    public Vector3 BottomLeftPos { get; private set; }
    public Vector3 TopRightPos { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one CameraFollow instance in scene!");
            return;
        }

        Camera = GetComponent<Camera>();
        Instance = this;
    }

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

        BottomLeftPos = _myTrans.position - new Vector3(Width / 2, Height / 2, 0);
        TopRightPos = _myTrans.position + new Vector3(Width / 2, Height / 2, 0);
    }

    public Vector2 GetCameraBtmLeftWorldPos()
    {
        var position = _myTrans.position;
        
        Debug.Log("Camera width: " + Width + ", height: " + Height);

        return new Vector2(
            position.x - Width / 2,
            position.y - Height / 2
            );
    }
}
