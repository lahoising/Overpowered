using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera cam = null;
    [SerializeField] private Animator anim = null;

    [SerializeField] private Rigidbody rb = null;
    private InputMaster input;

    private Vector2 xzMoveBuffer = Vector2.zero;
    [SerializeField] private float walkSpeed = 10.0f;
    [SerializeField] private float runSpeed = 30.0f;
    [SerializeField] private float rageSpeed = 100.0f;
    private float runMultiplier = 0.0f;
    
    private float yMoveBuffer = 0.0f;
    [SerializeField] private float verticalSpeed = 2.0f;

    [SerializeField] private float lookRotationSpeed = 10.0f;

    #if UNITY_EDITOR
    private bool lockMouse = false;
    #endif

    void Awake()
    {
        if(!rb) rb = GetComponent<Rigidbody>();
        if(!cam) cam = Camera.main;
        if(!anim) anim = GetComponent<Animator>();

        input = new InputMaster();
        input.Player.Movement.performed += InputMove;
        input.Player.Movement.canceled += InputMove;

        input.Player.Run.performed += InputRun;
        input.Player.Run.canceled += InputRun;

        input.Player.Jump.performed += InputJump;
        input.Player.Jump.canceled += InputJump;

        input.Player.Crouch.performed += InputCrouch;
        input.Player.Crouch.canceled += InputCrouch;
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
        Vector3 dir = new Vector3(xzMoveBuffer.x, 0.0f, xzMoveBuffer.y);
        dir = dir.x * right + dir.z * fwd;
        dir.y = yMoveBuffer;
        rb.velocity = dir * speed;

        if(xzMoveBuffer != Vector2.zero)
        {
            dir.y = 0.0f;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * lookRotationSpeed));
        }

        anim.SetFloat("fwdSpeed", rb.velocity.sqrMagnitude);
    }

    /// <param name="dir">The direction to float to. Will be clamped between -1.0f to 1.0f</param>
    private void SetFloatDir(float dir)
    {
        dir = Mathf.Clamp(dir, -1.0f, 1.0f);
        yMoveBuffer = dir * verticalSpeed;
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

    private void InputJump(InputAction.CallbackContext context)
    {
        SetFloatDir(context.ReadValue<float>());
    }

    private void InputCrouch(InputAction.CallbackContext context)
    {
        SetFloatDir(-context.ReadValue<float>());
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
