using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WalkingMovement : MonoBehaviour
{
    [Header("Movement speed")]
    public float movSpeed;
    public float climbSpeed;

    public float groundDrag;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Jump settings")]
    public float jumpForce;
    public float jumpCooldown = 1f;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Ground check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded = false;

    float hInput, vInput;
    bool isOnLadder = false;

    Vector2 movDirection;
    Vector3 climbDirection;

    public InputActionReference walkAction;
    public InputActionReference jumpAction;
    public InputActionReference sprintAction;
    public InputActionReference action;

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 2f, whatIsGround);

        LimitSpeed();

        rb.drag = grounded ? groundDrag : 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.GetComponent<Collider>().gameObject.CompareTag("ClimbSurface"))
        {
            climbDirection = collision.GetComponent<Collider>().transform.up;
            transform.Translate(new Vector3(collision.GetComponent<Collider>().transform.position.x - transform.position.x, 0f, 0f));
            rb.velocity = Vector3.zero;
            rb.useGravity = false;
            isOnLadder = true;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.GetComponent<Collider>().gameObject.CompareTag("ClimbSurface"))
        {
            rb.useGravity = true;
            isOnLadder = false;
        }
    }

    private void OnEnable()
    {
        walkAction.action.performed += Move;
        walkAction.action.canceled += Move;
        jumpAction.action.performed += Jump;
        sprintAction.action.started += Sprint;
        sprintAction.action.canceled += Sprint;
        PlayerEntity.onPlayerDeath += HandleDeath;
    }

    private void OnDisable()
    {
        walkAction.action.performed -= Move;
        walkAction.action.canceled -= Move;
        jumpAction.action.performed -= Jump;
        sprintAction.action.started -= Sprint;
        sprintAction.action.canceled -= Sprint;
        PlayerEntity.onPlayerDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        GetComponent<Rigidbody>().useGravity = true;
        Destroy(this);
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        movDirection = ctx.ReadValue<Vector2>();
    }

    public void Sprint(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            movSpeed *= 1.5f;
        if (ctx.canceled)
            movSpeed /= 1.5f;
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (!grounded || !readyToJump)
            return;

        readyToJump = false;
        rb.velocity.Set(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Acceleration);
        Invoke(nameof(ResetJump), jumpCooldown);
    }

    void TakeInput()
    {
        hInput = Input.GetAxisRaw("Horizontal");
        vInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(jumpKey) && readyToJump)
        {
            if(isOnLadder)
            {
                readyToJump = false;
                isOnLadder = false;
                rb.useGravity = true;
                rb.AddForce(-transform.forward*50f);
                //Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }
            else if(grounded)
            {
                readyToJump = false;
                //Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            movSpeed += 8f;
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            movSpeed -= 8f;
        }
    }

    void MovePlayer()
    {
        float x = movDirection.x;
        float y = movDirection.y;

        Vector3 resultForce = (transform.forward * y + transform.right * x) * movSpeed;
        rb.AddForce(resultForce, ForceMode.Acceleration);
    }

    private void LimitSpeed()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if(flatVel.magnitude > movSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * movSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
