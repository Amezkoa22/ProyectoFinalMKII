using UnityEngine;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    [Header("Identificación de Jugador")]
    public int playerNumber = 1;

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
    public GameObject hitboxLowPunch;
    public GameObject hitboxUpperCut;

    private Animator anim;
    private PlayerMovement movement;

    private int comboStep = 0;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private float comboResetTimer = 0f;

    private bool isKicking = false;
    private bool isKickOnCooldown = false;
    private float kickCooldownTimer = 0f;
    private float kickActiveTimer = 0f;

    private bool isLowPunching = false;
    private float lowPunchTimer = 0f;

    private bool isUpperCutting = false;
    private float upperCutTimer = 0f;

    private bool isDoingSpecial = false;
    private bool isSpecialOnCooldown = false;
    private float specialCooldownTimer = 0f;
    private float specialActiveTimer = 0f;

    private KeyCode keyPunch;
    private KeyCode keyUpperCut;
    private KeyCode keyKick;
    private KeyCode keyCrouchModifier;
    private KeyCode keyLeft;
    private KeyCode keyRight;

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

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        if (playerNumber == 1)
        {
            keyPunch = KeyCode.J;
            keyUpperCut = KeyCode.I;
            keyKick = KeyCode.L;
            keyCrouchModifier = KeyCode.S;
            keyLeft = KeyCode.A;
            keyRight = KeyCode.D;
        }
        else
        {
            keyPunch = KeyCode.K;
            keyUpperCut = KeyCode.O;
            keyKick = KeyCode.Semicolon;
            keyCrouchModifier = KeyCode.G;
            keyLeft = KeyCode.F;
            keyRight = KeyCode.H;
        }
    }

    void Update()
    {
        HandleCooldowns();

        if (isDoingSpecial) { HandleSpecialTimer(); return; }
        if (isLowPunching) { HandleLowPunchTimer(); return; }
        if (isUpperCutting) { HandleUpperCutTimer(); return; }

        if (movement.isGrounded && !Input.GetKey(keyCrouchModifier) && !isSpecialOnCooldown && !movement.isAttacking)
        {
            RecordSpecialInputs();
            if (CheckSpecialSequence()) { ExecuteGetOverHere(); return; }
        }

        if (movement.isGrounded && Input.GetKey(keyCrouchModifier) && !isKicking && !movement.isAttacking)
        {
            if (!movement.isBlocking)
            {
                if (Input.GetKeyDown(keyPunch)) ExecuteLowPunch();
                else if (Input.GetKeyDown(keyUpperCut)) ExecuteUpperCut();
            }
            return;
        }

        if (movement.isGrounded)
        {
            if (!isKicking)
            {
                if (Input.GetKeyDown(keyPunch) && !isOnCooldown && !movement.isBlocking) ExecuteMidPunch();
                if (Input.GetKeyDown(keyKick) && !isKickOnCooldown && !movement.isBlocking && !movement.isAttacking) DecideAndExecuteKick();
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
        if (Input.GetKeyDown(keyLeft)) inputHistory.Add(new InputCommand(keyLeft, Time.time));
        if (Input.GetKeyDown(keyRight)) inputHistory.Add(new InputCommand(keyRight, Time.time));
        if (Input.GetKeyDown(keyPunch)) inputHistory.Add(new InputCommand(keyPunch, Time.time));
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

        if (enemyToRight) { if (uno == keyLeft && dos == keyRight && tres == keyPunch) { inputHistory.Clear(); return true; } }
        else { if (uno == keyRight && dos == keyLeft && tres == keyPunch) { inputHistory.Clear(); return true; } }
        return false;
    }

    void ExecuteGetOverHere()
    {
        isDoingSpecial = true; movement.isAttacking = true;
        anim.Play("GetOverHere", 0, 0f); specialActiveTimer = specialDuration;
    }

    void HandleSpecialTimer()
    {
        specialActiveTimer -= Time.deltaTime;
        if (specialActiveTimer <= 0f) { isDoingSpecial = false; movement.isAttacking = false; anim.Play("Scorpion_Idle", 0, 0f); isSpecialOnCooldown = true; specialCooldownTimer = specialCooldownDuration; }
    }

    void ExecuteLowPunch()
    {
        isLowPunching = true; movement.isAttacking = true;
        anim.Play("LowPunch", 0, 0f); lowPunchTimer = lowPunchDuration;
    }

    void HandleLowPunchTimer()
    {
        lowPunchTimer -= Time.deltaTime;
        if (lowPunchTimer <= 0f) { isLowPunching = false; movement.isAttacking = false; anim.Play("Scorpion_Idle", 0, 0f); }
    }

    void ExecuteUpperCut()
    {
        isUpperCutting = true; movement.isAttacking = true;
        anim.Play("UpperCut", 0, 0f); upperCutTimer = upperCutDuration;
    }

    void HandleUpperCutTimer()
    {
        upperCutTimer -= Time.deltaTime;
        if (upperCutTimer <= 0f) { isUpperCutting = false; movement.isAttacking = false; anim.Play("Scorpion_Idle", 0, 0f); }
    }

    void ExecuteMidPunch()
    {
        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
        if (isKicking || isLowPunching || isUpperCutting || isDoingSpecial) return;
        if (movement.isAttacking && state.normalizedTime < 0.75f) { if (state.IsName("MidPunch_R_Start") || state.IsName("MidPunch_L_Loop") || state.IsName("MidPunch_R_Loop")) return; }
        movement.isAttacking = true; comboResetTimer = 0f;
        if (comboStep == 0) { anim.Play("MidPunch_R_Start", 0, 0f); comboStep = 1; }
        else if (comboStep == 1) { anim.Play("MidPunch_L_Loop", 0, 0f); comboStep = 2; }
        else if (comboStep == 2) { anim.Play("MidPunch_R_Loop", 0, 0f); comboStep = 3; }
    }

    void DecideAndExecuteKick()
    {
        isKicking = true; movement.isAttacking = true;
        float moveInput = 0f;
        if (Input.GetKey(keyRight)) moveInput = 1f;
        else if (Input.GetKey(keyLeft)) moveInput = -1f;

        bool movingTowardRival = false;
        if (movement.rival != null && moveInput != 0) { if ((movement.rival.position.x > transform.position.x && moveInput > 0) || (movement.rival.position.x < transform.position.x && moveInput < 0)) movingTowardRival = true; }

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
        else if (comboResetTimer > comboLimit || (state.normalizedTime >= 1.0f && !Input.GetKey(keyPunch))) ResetPunchCombo();
    }

    void StartPunchCooldown() { movement.isAttacking = false; comboStep = 0; isOnCooldown = true; cooldownTimer = cooldownDuration; }
    void ResetPunchCombo() { AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0); if (state.normalizedTime >= 0.9f || !state.IsName("MidPunch_R_Start")) { movement.isAttacking = false; comboStep = 0; } }

    void HandleCooldowns()
    {
        if (isOnCooldown) { cooldownTimer -= Time.deltaTime; if (cooldownTimer <= 0) isOnCooldown = false; }
        if (isKickOnCooldown) { kickCooldownTimer -= Time.deltaTime; if (kickCooldownTimer <= 0) isKickOnCooldown = false; }
        if (isSpecialOnCooldown) { specialCooldownTimer -= Time.deltaTime; if (specialCooldownTimer <= 0) isSpecialOnCooldown = false; }
    }

    public void CancelarAtaques()
    {
        isKicking = false;
        isLowPunching = false;
        isUpperCutting = false;
        isDoingSpecial = false;
        comboStep = 0;
        movement.isAttacking = false;

        DesactivarHitboxPunch();
        DesactivarHitboxKick();
        DesactivarHitboxLowPunch();
        DesactivarHitboxUpperCut();
    }

    public void ActivarHitboxPunch() { if (hitboxPunch != null) hitboxPunch.SetActive(true); }
    public void DesactivarHitboxPunch() { if (hitboxPunch != null) hitboxPunch.SetActive(false); }

    public void ActivarHitboxKick() { if (hitboxKick != null) hitboxKick.SetActive(true); }
    public void DesactivarHitboxKick() { if (hitboxKick != null) hitboxKick.SetActive(false); }

    public void ActivarHitboxLowPunch() { if (hitboxLowPunch != null) hitboxLowPunch.SetActive(true); }
    public void DesactivarHitboxLowPunch() { if (hitboxLowPunch != null) hitboxLowPunch.SetActive(false); }

    public void ActivarHitboxUpperCut() { if (hitboxUpperCut != null) hitboxUpperCut.SetActive(true); }
    public void DesactivarHitboxUpperCut() { if (hitboxUpperCut != null) hitboxUpperCut.SetActive(false); }
}