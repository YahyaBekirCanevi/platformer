using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private Text text;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] LayerMask groundLayer = 1;

    [Tooltip("Seconds of forgiveness: The amount of time after leaving a platform during which the player can still jump.")]
    [SerializeField] private float coyoteTime = 0.15f;
    private float coyoteTimer = 0f;

    private Rigidbody2D rb;
    private Transform cameraTransform;
    private Vector2 inputVector;
    private int direction = 1;
    private int score = 0;
    private bool jumpRequested = false;
    private bool jumpPressed = false;

    private bool IsGrounded
    {
        get => Physics2D.Raycast(transform.position, Vector2.down, .2f, groundLayer);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (Mathf.Abs(inputVector.x) > 0.1f)
        {
            int newDirection = inputVector.x > 0 ? 1 : -1;
            if (newDirection != direction)
            {
                direction = newDirection;
                Vector3 scale = spriteRenderer.transform.localScale;
                scale.x = direction;
                spriteRenderer.transform.localScale = scale;
            }
        }

        if (IsGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpPressed && coyoteTimer > 0f)
        {
            jumpRequested = true;
            coyoteTimer = 0f;
            jumpPressed = false;
        }

        animator.SetFloat("speed", IsGrounded ? rb.linearVelocity.magnitude : 0);

    }

    void LateUpdate()
    {
        Vector3 target = transform.position + new Vector3(0, 2, -10);
        target.x = Mathf.Round(target.x * 32) / 32f;
        target.y = Mathf.Round(target.y * 32) / 32f;
        cameraTransform.position = target;
    }

    void FixedUpdate()
    {
        // Horizontal movement
        Vector2 velocity = rb.linearVelocity;
        velocity.x = inputVector.x * moveSpeed;
        rb.linearVelocity = velocity;

        // Jump
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpRequested = false;
        }
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        moveAction.action.performed += OnMove;
        moveAction.action.canceled += OnMove;
    }

    void OnDisable()
    {
        moveAction.action.performed -= OnMove;
        moveAction.action.canceled -= OnMove;
        moveAction.action.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 newInput = context.ReadValue<Vector2>();

        // Detect jump press (fresh press)
        if (newInput.y > 0.1f && inputVector.y <= 0.1f)
        {
            jumpPressed = true;
        }

        inputVector = newInput;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Gold"))
        {
            score++;
            text.text = $"{score}";
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("End"))
        {
            Exit();
        }
    }

    private void Exit()
    {
        Debug.Log($"You Won! Score: {score}");
    }
}
