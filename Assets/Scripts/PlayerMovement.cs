using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private bool isPlayerOne = true; // Toggle in Inspector for Player 2

    private Rigidbody2D rb;
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();

        // Subscribe to the jump action
        inputActions.Player.Jump.performed += OnJump;
    }

    void OnDisable()
    {
        inputActions.Player.Disable();

        // Unsubscribe from the jump action
        inputActions.Player.Jump.performed -= OnJump;
    }

    void Update()
    {
        // Read the move input (WASD for now)
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // Apply horizontal movement
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        //Only allow jumping if the player is grounded
        if (isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    // Check for ground collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}