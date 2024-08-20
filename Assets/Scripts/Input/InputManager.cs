using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    public void OnEnterMenu()
    {
        Debug.Log("Entered menu");
        _playerInput.SwitchCurrentActionMap("Menu");
    }

    public void OnExitMenu()
    {
        Debug.Log("Exited menu");
        _playerInput.SwitchCurrentActionMap("Gameplay");
    }
}
