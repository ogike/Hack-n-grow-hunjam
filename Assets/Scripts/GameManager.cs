using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public enum GodModeType
    {
        Normal, GodMode, DemigodMode
    }

    public GodModeType CurrentGodModeType { get; private set; }
    
    public bool EnemyAttackEnabled { get; private set; }
    public bool EnemyMovementEnabled { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one game manager!");
            return;
        }
        Instance = this;

        CurrentGodModeType = GodModeType.Normal;
        EnemyAttackEnabled = true;
        EnemyMovementEnabled = true;
    }

    public void ToggleNormalMode() { CurrentGodModeType = GodModeType.Normal; }
    public void ToggleGodMode() { CurrentGodModeType = GodModeType.GodMode; }
    public void ToggleDemiGodMode() { CurrentGodModeType = GodModeType.DemigodMode; }
    
    public void ToggleEnemyAttackEnabled() { EnemyAttackEnabled = !EnemyAttackEnabled; }
    
    public void ToggleEnemyMovementEnabled() { EnemyMovementEnabled = !EnemyMovementEnabled; }

}
