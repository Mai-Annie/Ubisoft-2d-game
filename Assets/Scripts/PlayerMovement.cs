using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private bool isPlayerOne;

    private Rigidbody2D rb;
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 targetMoveInput;
    private bool isGrounded;
    private bool isAnchored;
    private float clumsinessModifier;
    private Bamboo heldBamboo;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new PlayerInputActions();
        inputActions.bindingMask = isPlayerOne
            ? InputBinding.MaskByGroup("PlayerOne")
            : InputBinding.MaskByGroup("PlayerTwo");
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Drop.performed += OnDrop;
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Drop.performed -= OnDrop;
    }

    void Update()
    {
        if (isAnchored) return;
        targetMoveInput = inputActions.Player.Move.ReadValue<Vector2>();
        // Clumsiness: sluggish input response when carrying bamboo
        float responsiveness = Mathf.Lerp(20f, 4f, clumsinessModifier);
        moveInput = Vector2.Lerp(moveInput, targetMoveInput, Time.deltaTime * responsiveness);
    }

    void FixedUpdate()
    {
        if (isAnchored) return;
        float effectiveSpeed = speed * (1f - clumsinessModifier * 0.4f);
        rb.linearVelocity = new Vector2(moveInput.x * effectiveSpeed, rb.linearVelocity.y);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && !isAnchored)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void OnDrop(InputAction.CallbackContext context)
    {
        heldBamboo?.ReleasePlayer(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = false;
    }

    public void SetAnchored(bool anchored)
    {
        isAnchored = anchored;
        if (anchored) moveInput = Vector2.zero;
    }

    public void SetClumsiness(float modifier)
    {
        clumsinessModifier = Mathf.Clamp01(modifier);
    }

    public void SetHeldBamboo(Bamboo bamboo)
    {
        heldBamboo = bamboo;
    }
}
