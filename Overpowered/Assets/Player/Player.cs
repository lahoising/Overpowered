using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera cam = null;
    [SerializeField] private Animator anim = null;

    [SerializeField] private Rigidbody rb = null;
    private InputMaster input;

    [SerializeField] private float walkSpeed = 10.0f;
    [SerializeField] private float runSpeed = 30.0f;
    [SerializeField] private float rageSpeed = 100.0f;
    [SerializeField] private float verticalSpeed = 25.0f;
    [SerializeField] private float lookRotationSpeed = 10.0f;
    private float runMultiplier = 0.0f;
    private Vector2 xzMoveBuffer = Vector2.zero;
    
    [SerializeField] private float jumpForce = 30.0f;
    private bool isGrounded = false;

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

    void FixedUpdate()
    {
        float speed = Mathf.Lerp(walkSpeed, runSpeed, runMultiplier);
        Vector3 fwd = cam.transform.TransformDirection(Vector3.forward);
        fwd.y = 0.0f;
        Vector3 right = cam.transform.TransformDirection(Vector3.right);
        Vector3 dir = new Vector3(xzMoveBuffer.x, 0.0f, xzMoveBuffer.y);
        dir = dir.x * right + dir.z * fwd;
        dir *= speed;
        dir.y = rb.velocity.y;
        rb.velocity = dir;

        if(xzMoveBuffer != Vector2.zero)
        {
            dir.y = 0.0f;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * lookRotationSpeed));
        }

        isGrounded = !isFloating && Physics.Raycast(transform.position, Vector3.down, 1.0f);

        anim.SetFloat("fwdSpeed", xzMoveBuffer.sqrMagnitude);
        anim.SetFloat("verticalSpeed", rb.velocity.y);
        anim.SetBool("floating", !rb.useGravity);
    }

    /// <param name="dir">The direction to float to. Will be clamped between -1.0f to 1.0f</param>
    private void SetFloatDir(float dir)
    {
        dir = Mathf.Clamp(dir, -1.0f, 1.0f);
        Vector3 vel = rb.velocity;
        vel.y = dir * verticalSpeed;
        rb.velocity = vel;
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
        if(context.interaction is UnityEngine.InputSystem.Interactions.MultiTapInteraction)
        {
            if(context.performed) 
            {
                rb.useGravity = false;
                SetFloatDir(0.0f);
            }
        }
        else
        {
            if(isFloating)
            {
                SetFloatDir(context.ReadValue<float>());
            }
            else if(isGrounded && context.performed)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }

    private void InputCrouch(InputAction.CallbackContext context)
    {
        if(context.interaction is UnityEngine.InputSystem.Interactions.MultiTapInteraction)
        {
            if(context.performed) 
            {
                rb.useGravity = true;
                SetFloatDir(0.0f);
            }
        }
        else
        {
            if(isFloating)
            {
                SetFloatDir(-context.ReadValue<float>());
            }
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

    public bool isFloating
    {
        get {return !rb.useGravity;}
    }
}
