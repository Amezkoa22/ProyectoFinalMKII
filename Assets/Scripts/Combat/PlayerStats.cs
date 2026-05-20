using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Configuración de Impacto")]
    public float stunDuration = 0.35f;
    public float knockbackForce = 5f;

    [Header("Configuración Fatality (Hit_UpperCut)")]
    [Tooltip("En qué punto de la animación se congelará (0.0 = inicio, 0.5 = mitad, 1.0 = final)")]
    [Range(0f, 1f)]
    public float freezeFrameNormalized = 0.5f;

    private PlayerMovement movement;
    private PlayerCombat combat;
    private Animator anim;
    private Rigidbody2D rb;

    private bool isStunned = false;
    private float stunTimer = 0f;
    private float currentKnockbackSpeed = 0f;

    private Manager_Rondas managerRondas;
    private bool isDizzy = false;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        anim = GetComponentInChildren<Animator>();

        // Nos aseguramos de restaurar la velocidad del Animator al iniciar la partida
        if (anim != null) anim.speed = 1f;

        rb = GetComponent<Rigidbody2D>();
        managerRondas = FindAnyObjectByType<Manager_Rondas>();
    }

    void Update()
    {
        if (isDizzy || isDead) return;
        if (isStunned)
        {
            movement.isAttacking = true;
            stunTimer -= Time.deltaTime;
            if (rb != null && currentKnockbackSpeed != 0f)
            {
                rb.position = new Vector2(rb.position.x + currentKnockbackSpeed * Time.deltaTime, rb.position.y);
            }
            currentKnockbackSpeed = Mathf.MoveTowards(currentKnockbackSpeed, 0f, Time.deltaTime * knockbackForce * 3f);
            if (stunTimer <= 0f)
            {
                isStunned = false;
                movement.isAttacking = false;
                currentKnockbackSpeed = 0f;
                if (anim != null) anim.Play("Scorpion_Idle", 0, 0f);
            }
        }
    }

    public void RecibirGolpe(int daño, AttackType tipoDeAtaque, Hurtbox.HurtboxType hurtboxImpactada)
    {
        if (isDead) return;

        // Lógica de reproducción de audio (Al rival)
        if (movement != null && movement.rival != null)
        {
            PlayerCombat combatRival = movement.rival.GetComponent<PlayerCombat>();
            if (combatRival != null)
            {
                combatRival.ReproducirSonidoImpacto(movement.isBlocking);
            }
        }

        // REACCIÓN AL GOLPE EN ESTADO DIZZY (FATALITY)
        if (isDizzy)
        {
            isDead = true;

            if (anim != null)
            {
                // 1. Forzamos la animación Hit_UpperCut en el frame exacto que definas
                anim.Play("Hit_UpperCut", 0, freezeFrameNormalized);

                // 2. Congelamos el tiempo del Animator para evitar que vuelva a Idle
                anim.speed = 0f;
            }

            AplicarKnockback();
            if (managerRondas != null) managerRondas.ReportarGolpeFatality(movement.playerNumber);
            return;
        }

        int dañoFinal = movement.isBlocking ? Mathf.RoundToInt(daño * 0.30f) : daño;
        currentHealth -= dañoFinal;

        ActualizarBarraUI();
        RevisarMuerte();

        if (isDead || isDizzy || movement.isBlocking) return;

        if (combat != null) combat.CancelarAtaques();
        isStunned = true;
        stunTimer = stunDuration;
        movement.isAttacking = true;

        if (hurtboxImpactada == Hurtbox.HurtboxType.Jump || (hurtboxImpactada == Hurtbox.HurtboxType.Normal && tipoDeAtaque == AttackType.Heavy))
        {
            anim.Play("Hit_UpperCut", 0, 0f);
            AplicarKnockback();
        }
        else if (hurtboxImpactada == Hurtbox.HurtboxType.Crouch) anim.Play("Hit_Duck", 0, 0f);
        else anim.Play("Hit_Stand", 0, 0f);
    }

    private void ActualizarBarraUI()
    {
        if (managerRondas != null)
        {
            float porcentaje = (float)currentHealth / maxHealth;
            managerRondas.ActualizarInterfazVida(movement.playerNumber, porcentaje);
        }
    }

    private void RevisarMuerte()
    {
        if (currentHealth <= 0 && !isDead && !isDizzy)
        {
            currentHealth = 0;
            if (combat != null) combat.CancelarAtaques();
            anim.Play("Hit_UpperCut", 0, 0f);
            AplicarKnockback();

            int victoriasEnemigo = movement.playerNumber == 1 ? Datos_Partida.victoriasJ2 : Datos_Partida.victoriasJ1;
            if (victoriasEnemigo >= 1) isDizzy = true;
            else isDead = true;

            if (managerRondas != null) managerRondas.ReportarMuerte(movement.playerNumber);

            // Apagamos los scripts de control
            if (movement != null) movement.enabled = false;
            if (combat != null) combat.enabled = false;
        }
    }

    private void AplicarKnockback()
    {
        if (movement.rival != null && rb != null)
        {
            float dir = transform.position.x > movement.rival.position.x ? 1f : -1f;
            currentKnockbackSpeed = dir * knockbackForce;
        }
    }
}