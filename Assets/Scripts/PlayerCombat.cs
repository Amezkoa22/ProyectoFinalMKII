using UnityEngine;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    [Header("Configuración del Puño de Pie")]
    public float comboLimit = 0.5f;
    public float cooldownDuration = 0.4f;

    [Header("Configuración de las Patadas")]
    public float kickCooldownDuration = 0.3f;
    public float kickStandingDuration = 0.9f;
    public float kickWalkingDuration = 0.4f;

    [Header("Configuración de Golpes Agachados")]
    public float lowPunchDuration = 0.5f;
    public float upperCutDuration = 0.6f;

    [Header("Ataque Especial: GetOverHere")]
    public float specialDuration = 1.2f;
    public float specialCooldownDuration = 6.0f;
    public float sequenceWindow = 0.4f;

    [Header("Referencias de Hitboxes de Ataque (Objetos Hijos)")]
    public GameObject hitboxPunch;
    public GameObject hitboxKick;
    public GameObject hitboxUpperCut;

    private Animator anim;
    private PlayerMovement movement;

    // Variables para el Puño de Pie
    private int comboStep = 0;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private float comboResetTimer = 0f;

    // Variables para la Patada
    private bool isKicking = false;
    private bool isKickOnCooldown = false;
    private float kickCooldownTimer = 0f;
    private float kickActiveTimer = 0f;

    // Variables para el Golpe Agachado (LowPunch)
    private bool isLowPunching = false;
    private float lowPunchTimer = 0f;

    // Variables para el Gancho Agachado (UpperCut)
    private bool isUpperCutting = false;
    private float upperCutTimer = 0f;

    // Variables para el Ataque Especial
    private bool isDoingSpecial = false;
    private bool isSpecialOnCooldown = false;
    private float specialCooldownTimer = 0f;
    private float specialActiveTimer = 0f;

    private struct InputCommand
    {
        public KeyCode key;
        public float time;
        public InputCommand(KeyCode k, float t) { key = k; time = t; }
    }
    private List<InputCommand> inputHistory = new List<InputCommand>();

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        HandleCooldowns();

        // 1. SI SE ESTÁ EJECUTANDO EL ATAQUE ESPECIAL (Prioridad Máxima)
        if (isDoingSpecial)
        {
            HandleSpecialTimer();
            return;
        }

        // 2. SI SE ESTÁ EJECUTANDO ALGO AGACHADO (Prioridad de estados activos)
        if (isLowPunching) { HandleLowPunchTimer(); return; }
        if (isUpperCutting) { HandleUpperCutTimer(); return; }

        // --- SISTEMA DE DETECCIÓN DE SECUENCIA (Solo de pie, sin atacar y en el suelo) ---
        if (movement.isGrounded && !Input.GetKey(KeyCode.S) && !isSpecialOnCooldown && !movement.isAttacking)
        {
            RecordSpecialInputs();
            if (CheckSpecialSequence())
            {
                ExecuteGetOverHere();
                return;
            }
        }

        // 3. CONTROL TOTAL DE AGACHADO (LIBERADO)
        if (movement.isGrounded && Input.GetKey(KeyCode.S) && !isKicking && !movement.isAttacking)
        {
            if (!movement.isBlocking)
            {
                if (Input.GetKeyDown(KeyCode.J)) ExecuteLowPunch();
                else if (Input.GetKeyDown(KeyCode.I)) ExecuteUpperCut();
            }
            return;
        }

        // 4. SI ESTÁ DE PIE NORMAL (Solo entra aquí si no se presionó la S)
        if (movement.isGrounded)
        {
            if (!isKicking)
            {
                // LÓGICA DEL PUÑO DE PIE (Tecla J)
                if (Input.GetKeyDown(KeyCode.J) && !isOnCooldown && !movement.isBlocking)
                {
                    ExecuteMidPunch();
                }

                // LÓGICA DE LAS PATADAS (Tecla L)
                if (Input.GetKeyDown(KeyCode.L) && !isKickOnCooldown && !movement.isBlocking && !movement.isAttacking)
                {
                    DecideAndExecuteKick();
                }
            }
            else
            {
                HandleKickTimer();
            }

            if (movement.isAttacking && !isKicking && !isLowPunching && !isUpperCutting && !isDoingSpecial)
            {
                CheckPunchStatus();
            }
        }
    }

    void RecordSpecialInputs()
    {
        if (Input.GetKeyDown(KeyCode.A)) inputHistory.Add(new InputCommand(KeyCode.A, Time.time));
        if (Input.GetKeyDown(KeyCode.D)) inputHistory.Add(new InputCommand(KeyCode.D, Time.time));
        if (Input.GetKeyDown(KeyCode.J)) inputHistory.Add(new InputCommand(KeyCode.J, Time.time));

        inputHistory.RemoveAll(cmd => Time.time - cmd.time > sequenceWindow);
    }

    bool CheckSpecialSequence()
    {
        if (inputHistory.Count < 3 || movement.rival == null) return false;

        int count = inputHistory.Count;
        KeyCode uno = inputHistory[count - 3].key;
        KeyCode dos = inputHistory[count - 2].key;
        KeyCode tres = inputHistory[count - 1].key;

        bool enemyToRight = movement.rival.position.x > transform.position.x;

        if (enemyToRight)
        {
            if (uno == KeyCode.A && dos == KeyCode.D && tres == KeyCode.J)
            {
                inputHistory.Clear();
                return true;
            }
        }
        else
        {
            if (uno == KeyCode.D && dos == KeyCode.A && tres == KeyCode.J)
            {
                inputHistory.Clear();
                return true;
            }
        }

        return false;
    }

    void ExecuteGetOverHere()
    {
        isDoingSpecial = true;
        movement.isAttacking = true;
        anim.Play("GetOverHere", 0, 0f);
        specialActiveTimer = specialDuration;
    }

    void HandleSpecialTimer()
    {
        specialActiveTimer -= Time.deltaTime;
        if (specialActiveTimer <= 0f)
        {
            isDoingSpecial = false;
            movement.isAttacking = false;
            anim.Play("Scorpion_Idle", 0, 0f);
            isSpecialOnCooldown = true;
            specialCooldownTimer = specialCooldownDuration;
        }
    }

    void ExecuteLowPunch()
    {
        isLowPunching = true; movement.isAttacking = true;
        anim.Play("LowPunch", 0, 0f); lowPunchTimer = lowPunchDuration;
    }

    void HandleLowPunchTimer()
    {
        lowPunchTimer -= Time.deltaTime;
        if (lowPunchTimer <= 0f)
        {
            isLowPunching = false; movement.isAttacking = false;
            if (Input.GetKey(KeyCode.S)) anim.Play("Crouch_In", 0, 1.0f); else anim.Play("Scorpion_Idle", 0, 0f);
        }
    }

    void ExecuteUpperCut()
    {
        isUpperCutting = true; movement.isAttacking = true;
        anim.Play("UpperCut", 0, 0f); upperCutTimer = upperCutDuration;
    }

    void HandleUpperCutTimer()
    {
        upperCutTimer -= Time.deltaTime;
        if (upperCutTimer <= 0f)
        {
            isUpperCutting = false; movement.isAttacking = false;
            if (Input.GetKey(KeyCode.S)) anim.Play("Crouch_In", 0, 1.0f); else anim.Play("Scorpion_Idle", 0, 0f);
        }
    }

    void ExecuteMidPunch()
    {
        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
        if (isKicking || isLowPunching || isUpperCutting || isDoingSpecial) return;
        if (movement.isAttacking && state.normalizedTime < 0.75f)
        {
            if (state.IsName("MidPunch_R_Start") || state.IsName("MidPunch_L_Loop") || state.IsName("MidPunch_R_Loop")) return;
        }
        movement.isAttacking = true; comboResetTimer = 0f;
        if (comboStep == 0) { anim.Play("MidPunch_R_Start", 0, 0f); comboStep = 1; }
        else if (comboStep == 1) { anim.Play("MidPunch_L_Loop", 0, 0f); comboStep = 2; }
        else if (comboStep == 2) { anim.Play("MidPunch_R_Loop", 0, 0f); comboStep = 3; }
    }

    void DecideAndExecuteKick()
    {
        isKicking = true; movement.isAttacking = true;
        float moveInput = Input.GetAxisRaw("Horizontal"); bool movingTowardRival = false;
        if (movement.rival != null && moveInput != 0)
        {
            if ((movement.rival.position.x > transform.position.x && moveInput > 0) || (movement.rival.position.x < transform.position.x && moveInput < 0)) movingTowardRival = true;
        }
        if (movingTowardRival) { anim.Play("Kick_Walking", 0, 0f); kickActiveTimer = kickWalkingDuration; }
        else { anim.Play("Kick_standing", 0, 0f); kickActiveTimer = kickStandingDuration; }
    }

    void HandleKickTimer()
    {
        kickActiveTimer -= Time.deltaTime;
        if (kickActiveTimer <= 0f) { isKicking = false; movement.isAttacking = false; if (!isKickOnCooldown) { isKickOnCooldown = true; kickCooldownTimer = kickCooldownDuration; } }
    }

    void CheckPunchStatus()
    {
        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0); comboResetTimer += Time.deltaTime;
        if (comboStep == 3 && state.normalizedTime >= 0.95f && state.IsName("MidPunch_R_Loop")) StartPunchCooldown();
        else if (comboResetTimer > comboLimit || (state.normalizedTime >= 1.0f && !Input.GetKey(KeyCode.J))) ResetPunchCombo();
    }

    void StartPunchCooldown() { movement.isAttacking = false; comboStep = 0; isOnCooldown = true; cooldownTimer = cooldownDuration; }
    void ResetPunchCombo() { AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0); if (state.normalizedTime >= 0.9f || !state.IsName("MidPunch_R_Start")) { movement.isAttacking = false; comboStep = 0; } }

    void HandleCooldowns()
    {
        if (isOnCooldown) { cooldownTimer -= Time.deltaTime; if (cooldownTimer <= 0) isOnCooldown = false; }
        if (isKickOnCooldown) { kickCooldownTimer -= Time.deltaTime; if (kickCooldownTimer <= 0) isKickOnCooldown = false; }
        if (isSpecialOnCooldown) { specialCooldownTimer -= Time.deltaTime; if (specialCooldownTimer <= 0) isSpecialOnCooldown = false; }
    }

    // ========================================================================
    // --- FUNCIONES PÚBLICAS DISPARADAS POR EVENTOS DE ANIMACIÓN ---
    // ========================================================================
    public void ActivarHitboxPunch() { if (hitboxPunch != null) hitboxPunch.SetActive(true); }
    public void DesactivarHitboxPunch() { if (hitboxPunch != null) hitboxPunch.SetActive(false); }

    public void ActivarHitboxKick() { if (hitboxKick != null) hitboxKick.SetActive(true); }
    public void DesactivarHitboxKick() { if (hitboxKick != null) hitboxKick.SetActive(false); }

    public void ActivarHitboxUpperCut() { if (hitboxUpperCut != null) hitboxUpperCut.SetActive(true); }
    public void DesactivarHitboxUpperCut() { if (hitboxUpperCut != null) hitboxUpperCut.SetActive(false); }
}