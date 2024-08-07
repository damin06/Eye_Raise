using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New Input Reader", menuName = "SO/Input/InputReader")]
public class InputReader : ScriptableObject, Controls.IPlayerActions 
{
    public event Action<Vector2> MovementEvent;
    public event Action<Vector2> AimPositionEvent;
    public event Action<float> MouseScrollEvent;
    public event Action SplitEvent;
    public event Action EmissionEvent;
    public event Action ShootEvent;
    private Controls controls;

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new Controls();
            controls.Player.SetCallbacks(this);
        }
        
        controls.Player.Enable();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        Vector2 _value = context.ReadValue<Vector2>();
        MovementEvent?.Invoke(_value);
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        Vector2 _value = context.ReadValue<Vector2>();
        AimPositionEvent?.Invoke(_value);
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ShootEvent?.Invoke();
        }
    }

    public void OnSplit(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SplitEvent?.Invoke();
        }
    }

    public void OnEmission(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            EmissionEvent?.Invoke();
        };
    }

    public void OnMouseScroll(InputAction.CallbackContext context)
    {
        float _value = context.ReadValue<float>();
        MouseScrollEvent?.Invoke(_value);
    }
}