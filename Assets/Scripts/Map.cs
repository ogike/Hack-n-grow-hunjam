using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static Map Instance { get; private set; }

    public Transform minPositionTransform;
    public Transform maxPositionTransform;

    public Transform minCameraPositionTransform;
    public Transform maxCameraPositionTransform;
    
    public Vector2 MinPosition { get; private set; }
    public Vector2 MaxPosition { get; private set; }

    public Vector2 MinCameraPosition { get; private set; }
    public Vector2 MaxCameraPosition { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple Map instances in this scene!");
            return;
        }
        Instance = this;
        
        MinPosition = minPositionTransform.position;
        MaxPosition = maxPositionTransform.position;
        
        MinCameraPosition = minCameraPositionTransform.position;
        MaxCameraPosition = maxCameraPositionTransform.position;
    }
}
