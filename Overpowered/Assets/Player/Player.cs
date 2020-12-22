using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera cam = null;

    [SerializeField] private Rigidbody rb = null;
    private InputMaster input;

    private Vector2 xzMoveBuffer = Vector2.zero;
    private float yMoveBuffer = 0.0f;
    [SerializeField] private float walkSpeed = 10.0f;
    [SerializeField] private float runSpeed = 30.0f;
    private float runMultiplier = 0.0f;

    [SerializeField] private float lookRotationSpeed = 10.0f;

    #if UNITY_EDITOR
    private bool lockMouse = false;
    #endif

    void Awake()
    {
        if(!rb) rb = GetComponent<Rigidbody>();
        if(!cam) cam = Camera.main;

        input = new InputMaster();
        input.Player.Movement.performed += InputMove;
        input.Player.Movement.canceled += InputMove;

        input.Player.Run.performed += InputRun;
        input.Player.Run.canceled += InputRun;
    }

    void Update()
    {
        #if UNITY_EDITOR
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            lockMouse = !lockMouse;
        }

        if(lockMouse)
        {
        #endif
            Mouse.current.WarpCursorPosition(Vector2.zero);
        #if UNITY_EDITOR
        }
        #endif
    }

    void FixedUpdate()
    {
        float speed = Mathf.Lerp(walkSpeed, runSpeed, runMultiplier);
        Vector3 fwd = cam.transform.TransformDirection(Vector3.forward);
        fwd.y = 0.0f;
        Vector3 right = cam.transform.TransformDirection(Vector3.right);
        Vector3 dir = new Vector3(xzMoveBuffer.x, yMoveBuffer, xzMoveBuffer.y);
        dir = dir.x * right + dir.z * fwd;
        rb.velocity = dir * speed;

        if(xzMoveBuffer != Vector2.zero)
        {
            dir.y = 0.0f;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * lookRotationSpeed);
        }
    }

    private void InputMove(InputAction.CallbackContext context)
    {
        Vector2 dir = context.ReadValue<Vector2>();
        xzMoveBuffer = dir;
    }

    private void InputRun(InputAction.CallbackContext context)
    {
        runMultiplier = context.ReadValue<float>();
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
