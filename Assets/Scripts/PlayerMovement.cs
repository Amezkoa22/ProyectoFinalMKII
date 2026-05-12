using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float walkSpeed = 4f;
    public float backwardSpeed = 3f;
    public float jumpForce = 12f;
    public float diagonalJumpSpeed = 5f;

    [Header("Referencias de Hitboxes")]
    public GameObject hitboxNormal;
    public GameObject hitboxCrouch;
    public GameObject hitboxJump;

    [Header("Referencias Generales")]
    public Transform rival;
    public Transform visualPart;
    public LayerMask groundLayer;

    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isAttacking;
    [HideInInspector] public bool isBlocking;

    private Rigidbody2D rb;
    private Animator anim;
    private float moveInput;
    private bool isCrouching;

    private float jumpTimer;
    private int currentJumpPhase = 0;
    private bool isDiagonalJump = false;
    private float lockedHorizontalSpeed = 0f;
    private int flipDirection = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        // Aseguramos un estado inicial
        UpdateHitboxes();
    }

    void Update()
    {
        Vector2 detectionCenter = new Vector2(transform.position.x, transform.position.y + 0.1f);
        isGrounded = Physics2D.OverlapCircle(detectionCenter, 0.2f, groundLayer);

        if (isGrounded)
        {
            if (rb.linearVelocity.y <= 0.1f) { isDiagonalJump = false; currentJumpPhase = 0; }
            if (!isAttacking) HandleInput();
        }
        else
        {
            moveInput = 0;
            isBlocking = false;
            isCrouching = false;
        }

        LookAtRival();
        UpdateHitboxes(); // Cambia la hitbox activa según el estado
        UpdateAnimations();
    }

    void HandleInput()
    {
        bool holdS = Input.GetKey(KeyCode.S);
        bool holdO = Input.GetKey(KeyCode.O);

        if (holdS)
        {
            isCrouching = true;
            isBlocking = holdO;
            moveInput = 0;
        }
        else if (holdO)
        {
            isBlocking = true;
            isCrouching = false;
            moveInput = 0;
        }
        else
        {
            isCrouching = false;
            isBlocking = false;
            moveInput = Input.GetAxisRaw("Horizontal");
        }

        if (Input.GetKeyDown(KeyCode.W) && !isCrouching && !isBlocking)
        {
            if (moveInput == 0) StartVerticalJump();
            else StartDiagonalJump(moveInput);
        }
    }

    // Lógica para prender y apagar los objetos de colisión
    void UpdateHitboxes()
    {
        if (!isGrounded)
        {
            hitboxJump.SetActive(true);
            hitboxNormal.SetActive(false);
            hitboxCrouch.SetActive(false);
        }
        else if (isCrouching)
        {
            hitboxJump.SetActive(false);
            hitboxNormal.SetActive(false);
            hitboxCrouch.SetActive(true);
        }
        else
        {
            hitboxJump.SetActive(false);
            hitboxNormal.SetActive(true);
            hitboxCrouch.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        if (isGrounded && !isCrouching && !isAttacking && !isBlocking)
        {
            float currentSpeed = CalculateSpeed();
            rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
        }
        else if (isGrounded && (isCrouching || isAttacking || isBlocking))
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else if (isDiagonalJump)
        {
            rb.linearVelocity = new Vector2(lockedHorizontalSpeed, rb.linearVelocity.y);
        }
    }

    void StartVerticalJump() { isDiagonalJump = false; rb.linearVelocity = new Vector2(0, jumpForce); currentJumpPhase = 1; }
    void StartDiagonalJump(float inputDir) { isDiagonalJump = true; lockedHorizontalSpeed = inputDir * diagonalJumpSpeed; rb.linearVelocity = new Vector2(lockedHorizontalSpeed, jumpForce); }

    // (Mantener el resto de funciones como LookAtRival, CalculateSpeed, etc.)
    void UpdateAnimations()
    {
        if (anim == null) return;
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isCrouching", isCrouching);
        anim.SetBool("isBlocking", isBlocking);
        // ... (resto de parámetros que ya configuramos)
    }
}