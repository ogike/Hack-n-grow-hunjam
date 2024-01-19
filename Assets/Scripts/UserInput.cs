using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    public static UserInput instance;
    
    public Vector2 MoveInput { get; private set; }
    
    public bool LightAttackPressedThisFrame { get; private set; }
    public bool LightAttackHeld { get; private set; }
    public bool LightAttackReleased { get; private set; }
    
    public bool HeavyAttackPressedThisFrame { get; private set; }
    public bool HeavyAttackHeld { get; private set; }
    public bool HeavyAttackReleased { get; private set; }
    
    public bool DefendPressedThisFrame { get; private set; }
    public bool DefendHeld { get; private set; }
    public bool DefendReleased { get; private set; }
    
    public bool MenuButtonPressedThisFrame { get; private set; }
    
    public bool DebugMenuButtonPressedThisFrame { get; private set; }
    public bool DebugEnemySpawnPressedThisFrame { get; private set; }
    
    private PlayerInput _playerInput;

    private InputAction _moveAction;
    private InputAction _lightAttackAction;
    private InputAction _heavyAttackAction;
    private InputAction _defendAction;
    
    private InputAction _menuAction;
    
    private InputAction _debugMenuAction;
    private InputAction _debugSpawnAction;
    
    

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one UserInput instance in scene!");
            return;
        }
        instance = this;

        _playerInput = GetComponent<PlayerInput>();
        
        _moveAction = _playerInput.actions["Move"];
        _lightAttackAction = _playerInput.actions["LightAttack"];
        _heavyAttackAction = _playerInput.actions["HeavyAttack"];
        _defendAction = _playerInput.actions["Defend"];
        _menuAction = _playerInput.actions["MenuOpenClose"];
        _debugMenuAction = _playerInput.actions["DebugMenu"];
        _debugSpawnAction = _playerInput.actions["DebugSpawnEnemy"];
    }

    private void Update()
    {
        MoveInput = _moveAction.ReadValue<Vector2>();

        LightAttackPressedThisFrame = _lightAttackAction.WasPressedThisFrame();
        LightAttackHeld = _lightAttackAction.IsPressed();
        LightAttackReleased = _lightAttackAction.WasReleasedThisFrame();
        
        HeavyAttackPressedThisFrame = _heavyAttackAction.WasPressedThisFrame();
        HeavyAttackHeld = _heavyAttackAction.IsPressed();
        HeavyAttackReleased = _heavyAttackAction.WasReleasedThisFrame();

        DefendPressedThisFrame = _defendAction.WasPressedThisFrame();
        DefendHeld = _defendAction.IsPressed();
        DefendReleased = _defendAction.WasReleasedThisFrame();

        MenuButtonPressedThisFrame = _menuAction.WasPressedThisFrame();
        
        DebugMenuButtonPressedThisFrame = _debugMenuAction.WasPressedThisFrame();
        DebugEnemySpawnPressedThisFrame = _debugSpawnAction.WasPressedThisFrame();
    }
}
