using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Configuración de Impacto")]
    public float stunDuration = 0.35f;
    public float knockbackForce = 5f;

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

                if (anim != null)
                {
                    anim.Play("Scorpion_Idle", 0, 0f);
                }
            }
        }
    }

    public void RecibirGolpe(int daño, AttackType tipoDeAtaque, Hurtbox.HurtboxType hurtboxImpactada)
    {
        if (isDead) return;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dizzy") || isDizzy)
        {
            isDead = true;
            anim.Play("Hit_UpperCut", 0, 0f);
            AplicarKnockback();
            if (managerRondas != null) managerRondas.ReportarGolpeFatality(movement.playerNumber);
            return;
        }

        if (movement.isBlocking)
        {
            currentHealth -= Mathf.RoundToInt(daño * 0.30f);
            RevisarMuerte();
            return;
        }

        currentHealth -= daño;
        RevisarMuerte();

        if (isDead) return;

        if (combat != null) combat.CancelarAtaques();

        isStunned = true;
        stunTimer = stunDuration;
        movement.isAttacking = true;

        if (hurtboxImpactada == Hurtbox.HurtboxType.Jump)
        {
            anim.Play("Hit_UpperCut", 0, 0f);
            AplicarKnockback();
        }
        else if (hurtboxImpactada == Hurtbox.HurtboxType.Crouch)
        {
            anim.Play("Hit_Duck", 0, 0f);
            currentKnockbackSpeed = 0f;
        }
        else if (hurtboxImpactada == Hurtbox.HurtboxType.Normal)
        {
            if (tipoDeAtaque == AttackType.Heavy)
            {
                anim.Play("Hit_UpperCut", 0, 0f);
                AplicarKnockback();
            }
            else
            {
                anim.Play("Hit_Stand", 0, 0f);
                currentKnockbackSpeed = 0f;
            }
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

            if (managerRondas != null)
            {
                int winsEnemigo = movement.playerNumber == 1 ? Datos_Partida.victoriasJ2 : Datos_Partida.victoriasJ1;
                if (winsEnemigo >= 1)
                {
                    isDizzy = true;
                }
                else
                {
                    isDead = true;
                }

                managerRondas.ReportarMuerte(movement.playerNumber);
            }
        }
    }

    private void AplicarKnockback()
    {
        if (movement.rival != null && rb != null)
        {
            float direccion = transform.position.x > movement.rival.position.x ? 1f : -1f;
            currentKnockbackSpeed = direccion * knockbackForce;
        }
    }
}