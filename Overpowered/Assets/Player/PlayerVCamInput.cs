using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVCamInput : MonoBehaviour, Cinemachine.AxisState.IInputAxisProvider
{
    private Vector3 buffer = Vector3.zero;
    [Range(1.0f, 20.0f)]
    public float sensitivity = 5.0f;
    
    private InputMaster input;

    void Awake()
    {
        input = new InputMaster();
        input.Player.Look.performed += InputLook;
        input.Player.Look.canceled += InputLook;
    }

    private void InputLook(InputAction.CallbackContext context)
    {
        Vector2 look = context.ReadValue<Vector2>() * sensitivity;
        buffer.x = look.x;
        buffer.y = look.y;
    }

    public virtual float GetAxisValue(int axis)
    {
        switch(axis)
        {
        case 0: return buffer.x;
        case 1: return buffer.y;
        case 2: return buffer.z;
        default: return 0.0f;
        }
    }

    void OnEnable()
    {
        input.Enable();
    }

    void OnDisable()
    {
        input.Disable();
    }
}
