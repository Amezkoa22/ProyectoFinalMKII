using UnityEngine;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    [Header("Identificación de Jugador")]
    public int playerNumber = 1;

    [Header("Identidad del Personaje (Inspector Check)")]
    public bool esScorpion = false;
    public bool esSubZero = false;
    public bool esKitana = false;

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

    [Header("Ataque Especial Común")]
    public float specialCooldownDuration = 6.0f;
    public float sequenceWindow = 0.4f;

    [Header("Ataque Especial: Scorpion (Spear)")]
    public Transform spearTransform;
    public float spearGrowthSpeed = 25f;
    public float maxSpearScale = 15f;
    public float pullSpeed = 15f;
    public float pullStopDistance = 1.5f;

    [Header("Ataque Especial: Sub-Zero (Ice Ball)")]
    public GameObject iceBallPrefab;
    public Transform spawnPointProyectil;
    public float iceBallSpeed = 12f;
    public float iceFreezeDuration = 3.0f;

    [Header("Referencias de Hitboxes de Ataque")]
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

    // Cambiado a public para que PlayerMovement pueda leerlo
    public bool isFrozen = false;
    private float freezeTimer = 0f;
    public bool estaCongeladoVisualmente = false; // Cambiado a public por corrección

    private bool isSpearExtending = false;
    private bool isSpearRetracting = false;
    private bool hasSpearHit = false;

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

        if (spearTransform != null)
        {
            spearTransform.localScale = new Vector3(0, spearTransform.localScale.y, spearTransform.localScale.z);
            spearTransform.gameObject.SetActive(false);
        }

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
        if (isFrozen)
        {
            HandleFreezeTimer();
            return;
        }

        HandleCooldowns();

        if (isDoingSpecial)
        {
            if (esScorpion) HandleSpearLogic();
            return;
        }

        if (isLowPunching) { HandleLowPunchTimer(); return; }
        if (isUpperCutting) { HandleUpperCutTimer(); return; }

        if (movement.isGrounded && !isSpecialOnCooldown && !movement.isAttacking)
        {
            RecordSpecialInputs();
            if (CheckSpecialSequence()) return;
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
        if (Input.GetKeyDown(keyCrouchModifier)) inputHistory.Add(new InputCommand(keyCrouchModifier, Time.time));
        if (Input.GetKeyDown(keyPunch)) inputHistory.Add(new InputCommand(keyPunch, Time.time));
        if (Input.GetKeyDown(keyKick)) inputHistory.Add(new InputCommand(keyKick, Time.time));

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

        if (esScorpion)
        {
            if (enemyToRight)
            {
                if (uno == keyLeft && dos == keyRight && tres == keyPunch) { inputHistory.Clear(); ExecuteGetOverHere(); return true; }
            }
            else
            {
                if (uno == keyRight && dos == keyLeft && tres == keyPunch) { inputHistory.Clear(); ExecuteGetOverHere(); return true; }
            }
        }

        if (esSubZero)
        {
            if (enemyToRight)
            {
                if (uno == keyCrouchModifier && dos == keyRight && tres == keyPunch) { inputHistory.Clear(); ExecuteIceBall(); return true; }
            }
            else
            {
                if (uno == keyCrouchModifier && dos == keyLeft && tres == keyPunch) { inputHistory.Clear(); ExecuteIceBall(); return true; }
            }
        }

        return false;
    }

    void ExecuteIceBall()
    {
        isDoingSpecial = true;
        movement.isAttacking = true;
        anim.Play("IceBall", 0, 0f);
    }

    public void LanzarIceBall()
    {
        if (iceBallPrefab == null) return;

        bool enemyToRight = true;
        if (movement.rival != null)
        {
            enemyToRight = movement.rival.position.x > transform.position.x;
        }

        Vector3 spawnPos = spawnPointProyectil != null ? spawnPointProyectil.position : transform.position + new Vector3(enemyToRight ? 1f : -1f, 0.5f, 0f);
        GameObject proyectilGo = Instantiate(iceBallPrefab, spawnPos, Quaternion.identity);
        IceBall scriptBola = proyectilGo.GetComponent<IceBall>();

        if (scriptBola != null)
        {
            scriptBola.Inicializar(iceBallSpeed, iceFreezeDuration, transform, enemyToRight);
        }

        isDoingSpecial = false;
        movement.isAttacking = false;
        isSpecialOnCooldown = true;
        specialCooldownTimer = specialCooldownDuration;
    }

    public void TerminarHabilidadIceBall()
    {
        isDoingSpecial = false;
        movement.isAttacking = false;
    }

    public void CongelarPorHielo(float duracion)
    {
        CancelarAtaques();
        isFrozen = true;
        freezeTimer = duracion;
        estaCongeladoVisualmente = true; // Agregado para PlayerMovement

        if (movement != null)
        {
            movement.isAttacking = true;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        if (anim != null)
        {
            anim.speed = 1f;
            anim.Play("Hit_Ice", 0, 0f);
        }

        Invoke("PausarAnimacionCongelado", 0.05f);
    }

    void PausarAnimacionCongelado()
    {
        if (isFrozen && anim != null)
        {
            anim.speed = 0f;
        }
    }

    void HandleFreezeTimer()
    {
        freezeTimer -= Time.deltaTime;
        if (freezeTimer <= 0f)
        {
            isFrozen = false;
            estaCongeladoVisualmente = false; // Agregado para PlayerMovement

            if (anim != null)
            {
                anim.speed = 1f;
                // Ajustado a Idle general para que funcione con SubZero clones
                anim.Play("Scorpion_Idle", 0, 0f);
            }
            if (movement != null)
            {
                movement.isAttacking = false;
            }
        }
    }

    void ExecuteGetOverHere()
    {
        isDoingSpecial = true;
        movement.isAttacking = true;
        isSpearExtending = false;
        isSpearRetracting = false;
        hasSpearHit = false;
        anim.Play("GetOverHere", 0, 0f);
    }

    public void DispararSpear()
    {
        if (spearTransform == null) return;

        if (movement != null && movement.rival != null)
        {
            bool enemyToRight = movement.rival.position.x > transform.position.x;
            spearTransform.rotation = Quaternion.Euler(0f, enemyToRight ? 0f : 180f, 0f);
        }

        spearTransform.gameObject.SetActive(true);
        spearTransform.localScale = new Vector3(0, spearTransform.localScale.y, spearTransform.localScale.z);

        isSpearExtending = true;
        isSpearRetracting = false;
        hasSpearHit = false;
        anim.speed = 0f;
    }

    void HandleSpearLogic()
    {
        if (!isSpearExtending && !isSpearRetracting) return;

        if (isSpearExtending)
        {
            spearTransform.localScale += new Vector3(spearGrowthSpeed * Time.deltaTime, 0, 0);
            SpriteRenderer sr = spearTransform.GetComponent<SpriteRenderer>();
            float currentSpearLength = sr != null ? sr.bounds.size.x : Mathf.Abs(spearTransform.lossyScale.x);
            float distanceToRival = movement.rival != null ? Mathf.Abs(movement.rival.position.x - transform.position.x) : float.MaxValue;

            if (currentSpearLength >= distanceToRival)
            {
                if (movement.rival != null)
                {
                    PlayerMovement rivalMove = movement.rival.GetComponent<PlayerMovement>();
                    PlayerStats rivalStats = movement.rival.GetComponent<PlayerStats>();

                    if (rivalMove != null && !rivalMove.isBlocking)
                    {
                        hasSpearHit = true;
                        isSpearExtending = false;
                        isSpearRetracting = true;
                        if (rivalStats != null) rivalStats.RecibirGolpe(5, AttackType.Mid, Hurtbox.HurtboxType.Normal);
                    }
                    else
                    {
                        isSpearExtending = false;
                        isSpearRetracting = true;
                    }
                }
            }
            else if (spearTransform.localScale.x >= maxSpearScale)
            {
                isSpearExtending = false;
                isSpearRetracting = true;
            }
        }
        else if (isSpearRetracting)
        {
            spearTransform.localScale -= new Vector3(spearGrowthSpeed * Time.deltaTime, 0, 0);

            if (hasSpearHit && movement.rival != null)
            {
                Rigidbody2D rivalRb = movement.rival.GetComponent<Rigidbody2D>();
                if (rivalRb != null)
                {
                    float direction = transform.position.x < movement.rival.position.x ? 1f : -1f;
                    float targetX = transform.position.x + (direction * pullStopDistance);

                    float effectivePullSpeed = pullSpeed;
                    if (spearTransform.localScale.x > 0f)
                    {
                        float distanceLeft = Mathf.Abs(rivalRb.position.x - targetX);
                        float timeLeft = spearTransform.localScale.x / spearGrowthSpeed;
                        effectivePullSpeed = distanceLeft / timeLeft;
                    }

                    rivalRb.position = Vector2.MoveTowards(rivalRb.position, new Vector2(targetX, rivalRb.position.y), effectivePullSpeed * Time.deltaTime);
                }
            }

            if (spearTransform.localScale.x <= 0)
            {
                if (hasSpearHit && movement.rival != null)
                {
                    Rigidbody2D rivalRb = movement.rival.GetComponent<Rigidbody2D>();
                    if (rivalRb != null)
                    {
                        float direction = transform.position.x < movement.rival.position.x ? 1f : -1f;
                        float targetX = transform.position.x + (direction * pullStopDistance);
                        rivalRb.position = new Vector2(targetX, rivalRb.position.y);
                    }
                }

                spearTransform.localScale = new Vector3(0, spearTransform.localScale.y, spearTransform.localScale.z);
                spearTransform.gameObject.SetActive(false);

                isSpearRetracting = false;
                hasSpearHit = false;
                isDoingSpecial = false;
                movement.isAttacking = false;

                anim.speed = 1f;
                anim.Play("Scorpion_Idle", 0, 0f);

                isSpecialOnCooldown = true;
                specialCooldownTimer = specialCooldownDuration;
            }
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

        if (spearTransform != null) spearTransform.gameObject.SetActive(false);
        if (anim != null) anim.speed = 1f;

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