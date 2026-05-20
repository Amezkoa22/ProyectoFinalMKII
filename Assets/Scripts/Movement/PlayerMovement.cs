using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Identificación de Jugador")]
    public int playerNumber = 1;

    [Header("Configuración de Movimiento")]
    public float walkSpeed = 4f;
    public float backwardSpeed = 3f;
    public float jumpForce = 12f;
    public float diagonalJumpSpeed = 5f;

    [Header("Tiempos Salto Vertical")]
    public float timeInFrame1 = 0.08f;
    public float timeInFrame2 = 0.1f;

    [Header("Referencias de Hurtboxes (Hijos con Trigger)")]
    public GameObject hurtboxNormal;
    public GameObject hurtboxCrouch;
    public GameObject hurtboxJump;

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

    private KeyCode keyUp;
    private KeyCode keyLeft;
    private KeyCode keyDown;
    private KeyCode keyRight;
    private KeyCode keyBlock;

    private PlayerCombat combatScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        combatScript = GetComponent<PlayerCombat>();

        UpdateHurtboxes();

        if (playerNumber == 1)
        {
            keyUp = KeyCode.W;
            keyLeft = KeyCode.A;
            keyDown = KeyCode.S;
            keyRight = KeyCode.D;
            keyBlock = KeyCode.N;
        }
        else
        {
            keyUp = KeyCode.T;
            keyLeft = KeyCode.F;
            keyDown = KeyCode.G;
            keyRight = KeyCode.H;
            keyBlock = KeyCode.M;
        }
    }

    void Update()
    {
        if (combatScript != null && combatScript.isFrozen) return;

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
        if (!isDiagonalJump) HandleVerticalJumpTimer();

        UpdateHurtboxes();
        UpdateAnimations();
    }

    void HandleInput()
    {
        bool holdS = Input.GetKey(keyDown);
        bool holdBlock = Input.GetKey(keyBlock);

        if (holdS)
        {
            isCrouching = true;
            isBlocking = holdBlock;
            moveInput = 0;
        }
        else if (holdBlock)
        {
            isBlocking = true;
            isCrouching = false;
            moveInput = 0;
        }
        else
        {
            isCrouching = false;
            isBlocking = false;

            if (Input.GetKey(keyRight)) moveInput = 1f;
            else if (Input.GetKey(keyLeft)) moveInput = -1f;
            else moveInput = 0f;
        }

        if (Input.GetKeyDown(keyUp) && !isCrouching && !isBlocking)
        {
            if (moveInput == 0) StartVerticalJump();
            else StartDiagonalJump(moveInput);
        }
    }

    void UpdateHurtboxes()
    {
        if (!isGrounded)
        {
            hurtboxJump.SetActive(true);
            hurtboxNormal.SetActive(false);
            hurtboxCrouch.SetActive(false);
        }
        else if (isCrouching)
        {
            hurtboxJump.SetActive(false);
            hurtboxNormal.SetActive(false);
            hurtboxCrouch.SetActive(true);
        }
        else
        {
            hurtboxJump.SetActive(false);
            hurtboxNormal.SetActive(true);
            hurtboxCrouch.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        if (combatScript != null && combatScript.isFrozen)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

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

    void StartVerticalJump()
    {
        isDiagonalJump = false;
        rb.linearVelocity = new Vector2(0, jumpForce);
        currentJumpPhase = 1;
        jumpTimer = 0f;
    }

    void StartDiagonalJump(float inputDir)
    {
        isDiagonalJump = true;
        lockedHorizontalSpeed = inputDir * diagonalJumpSpeed;
        rb.linearVelocity = new Vector2(lockedHorizontalSpeed, jumpForce);
        bool movingForward = (rival.position.x > transform.position.x && inputDir > 0) || (rival.position.x < transform.position.x && inputDir < 0);
        flipDirection = movingForward ? 1 : -1;
    }

    void HandleVerticalJumpTimer()
    {
        if (currentJumpPhase == 0) return;
        jumpTimer += Time.deltaTime;
        switch (currentJumpPhase)
        {
            case 1: if (jumpTimer >= timeInFrame1) { currentJumpPhase = 2; jumpTimer = 0f; } break;
            case 2: if (jumpTimer >= timeInFrame2) { currentJumpPhase = 3; jumpTimer = 0f; } break;
            case 3: if (rb.linearVelocity.y < -1f) { currentJumpPhase = 4; jumpTimer = 0f; } break;
            case 4: if (rb.linearVelocity.y < -8f || jumpTimer >= timeInFrame2) { currentJumpPhase = 5; jumpTimer = 0f; } break;
        }
    }

    void LookAtRival()
    {
        if (rival != null && isGrounded && !isBlocking)
        {
            bool ignorarFlipping = isAttacking || (combatScript != null && combatScript.estaCongeladoVisualmente);

            if (!ignorarFlipping)
            {
                if (rival.position.x > transform.position.x) visualPart.localScale = Vector3.one;
                else visualPart.localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    float CalculateSpeed()
    {
        if (moveInput == 0) return 0;
        bool movingForward = (rival.position.x > transform.position.x && moveInput > 0) || (rival.position.x < transform.position.x && moveInput < 0);
        return movingForward ? walkSpeed : backwardSpeed;
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isCrouching", isCrouching);
        anim.SetBool("isBlocking", isBlocking);

        if (!isDiagonalJump)
        {
            anim.SetInteger("JumpPhase", currentJumpPhase);
            anim.SetBool("isDiagonalJump", false);
        }
        else
        {
            anim.SetInteger("JumpPhase", 0);
            anim.SetBool("isDiagonalJump", true);
            anim.SetInteger("FlipDirection", flipDirection);
        }

        if (isGrounded && !isCrouching && !isAttacking && !isBlocking)
        {
            bool movingForward = (rival.position.x > transform.position.x && moveInput > 0) || (rival.position.x < transform.position.x && moveInput < 0);
            anim.SetFloat("MoveInput", moveInput == 0 ? 0 : (movingForward ? 1 : -1));
        }
        else
        {
            anim.SetFloat("MoveInput", 0);
        }
    }
}