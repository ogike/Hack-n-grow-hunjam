using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class EnemySpawnerCell
{
    public int xIndex { get; private set; }
    public int yIndex { get; private set; }

    public Vector2 BottomLeftPos { get; private set; }
    public Vector2 TopRightPos { get; private set; }
    
    public Vector2 CenterPos { get; private set; }
    

    public float width;
    public float height;

    public EnemySpawnerCell(int x, int y, Vector2 bottomLeft, float _width, float _height)
    {
        xIndex = x;
        yIndex = y;
        BottomLeftPos = bottomLeft;
        width = _width;
        height = _height;
        
        TopRightPos = new Vector2(BottomLeftPos.x + width, BottomLeftPos.y + height);
        CenterPos = new Vector2(BottomLeftPos.x + width / 2.0f, BottomLeftPos.y + height / 2.0f);

    }

    public Vector2 RandomPositionInside()
    {
        return new Vector2(
            Random.Range(BottomLeftPos.x, BottomLeftPos.x + width),
            Random.Range(BottomLeftPos.y, BottomLeftPos.y + height)
        );
    }

    public bool IsInCameraView(ref Vector2 cameraBtmLeftPos, ref Vector2 cameraTopRightPos)
    {
        //if cell is too much to the left or right
        if (TopRightPos.x < cameraBtmLeftPos.x || cameraTopRightPos.x < BottomLeftPos.x) return false;
        
        //if cell is too high or low
        if (cameraTopRightPos.y < BottomLeftPos.y || TopRightPos.y < cameraBtmLeftPos.y) return false;
        
        //right side
        if (TopRightPos.x < cameraBtmLeftPos.x && BottomLeftPos.x < cameraTopRightPos.x) return true;
        
        //left side
        // if (BottomLeftPos < TopRightPos.x && BottomLeftPos.x < cameraTopRightPos.x) return true;

        return false;
    }
    
}
