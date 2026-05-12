using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Configuración del Combo")]
    public float comboLimit = 0.5f;
    public float cooldownDuration = 0.4f;

    private Animator anim;
    private PlayerMovement movement;

    private int comboStep = 0;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private float comboResetTimer = 0f;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (movement.isGrounded)
        {
            HandleCooldown();

            // Bloqueo de ataque si se está defendiendo
            if (Input.GetKeyDown(KeyCode.J) && !isOnCooldown && !movement.isBlocking)
            {
                ExecuteMidPunch();
            }

            if (movement.isAttacking)
            {
                CheckComboStatus();
            }
        }
    }

    void ExecuteMidPunch()
    {
        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
        if (movement.isAttacking && state.normalizedTime < 0.75f)
        {
            if (state.IsName("MidPunch_R_Start") || state.IsName("MidPunch_L_Loop") || state.IsName("MidPunch_R_Loop"))
                return;
        }

        movement.isAttacking = true;
        comboResetTimer = 0f;

        if (comboStep == 0) { anim.Play("MidPunch_R_Start", 0, 0f); comboStep = 1; }
        else if (comboStep == 1) { anim.Play("MidPunch_L_Loop", 0, 0f); comboStep = 2; }
        else if (comboStep == 2) { anim.Play("MidPunch_R_Loop", 0, 0f); comboStep = 3; }
    }

    void CheckComboStatus()
    {
        comboResetTimer += Time.deltaTime;
        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);

        if (comboStep == 3 && state.normalizedTime >= 0.95f && state.IsName("MidPunch_R_Loop"))
        {
            StartCooldown();
        }
        else if (comboResetTimer > comboLimit || (state.normalizedTime >= 1.0f && !Input.GetKey(KeyCode.J)))
        {
            ResetCombo();
        }
    }

    void StartCooldown()
    {
        movement.isAttacking = false;
        comboStep = 0;
        isOnCooldown = true;
        cooldownTimer = cooldownDuration;
    }

    void ResetCombo()
    {
        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
        if (state.normalizedTime >= 0.9f || !state.IsName("MidPunch_R_Start"))
        {
            movement.isAttacking = false;
            comboStep = 0;
        }
    }

    void HandleCooldown()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0) isOnCooldown = false;
        }
    }
}