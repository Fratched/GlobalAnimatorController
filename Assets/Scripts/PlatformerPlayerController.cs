using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(PlatformerCharacterAnimator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlatformerPlayerController : MonoBehaviour
{
    public const string groundLayerName = "Ground";
    private PlatformerCharacterAnimator animator;
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private Transform groundCheck;
    private LayerMask groundLayer;
    private float horizontal;
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [Header("Camera")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    public CinemachineCamera CinemachineCamera => cinemachineCamera;
    void Awake()
    {
        animator = GetComponent<PlatformerCharacterAnimator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        // Resolve ground layer
        int layerIndex = LayerMask.NameToLayer(groundLayerName);
        if (layerIndex == -1)
        {
            Debug.LogError($"Layer \"{groundLayerName}\" does not exist. Falling back to Everything.");
            groundLayer = ~0;
        }
        else
        {
            groundLayer = 1 << layerIndex;
        }
        // Find ground detector child
        Transform detector = transform.Find("groundDetector");
        if (detector == null)
            Debug.LogError($"No child named \"groundDetector\" found under {gameObject.name}. Ground detection will not work.");
        else
            groundCheck = detector;
        col.isTrigger = false;
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        animator.SetSpeed(horizontal);
        animator.SetDirection(horizontal);
        animator.SetGrounded(IsGrounded());
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
            Jump();
    }
    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
    }
    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        animator.TriggerJump();
    }
    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    private void OnDrawGizmosSelected()
    {
        Transform detector = transform.Find("groundDetector");
        if (detector == null) return;
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawSphere(detector.position, groundCheckRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(detector.position, groundCheckRadius);
    }
}