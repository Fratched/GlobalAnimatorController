using UnityEngine;

[RequireComponent(typeof(SpriteAnimator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class UniversalCharacterController : MonoBehaviour
{
    private SpriteAnimator animator;
    private Rigidbody2D rb;
    private BoxCollider2D col;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private float horizontal;

    void Awake()
    {
        animator = GetComponent<SpriteAnimator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        // ✅ Ensure collider blocks
        col.isTrigger = false;

        // ✅ Rigidbody setup
        rb.gravityScale = 3f;
        rb.freezeRotation = true;

        // Optional but helps prevent clipping through objects
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        // INPUT
        horizontal = Input.GetAxis("Horizontal");

        // ANIMATION
        animator.SetSpeed(horizontal);
        animator.SetDirection(horizontal);

        // JUMP
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Jump();
        }

        // GROUNDED STATE
        animator.SetGrounded(IsGrounded());
    }

    void FixedUpdate()
    {
        // MOVEMENT
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        animator.TriggerJump();
    }

    private bool IsGrounded()
    {
        if (groundCheck == null)
        {
            Debug.LogWarning("GroundCheck not assigned!");
            return false;
        }

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // ✅ Debug ground check in scene view
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}