using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Configuración de Impacto")]
    public float stunDuration = 0.35f;
    public float knockbackForce = 5f;  // Modifica esto en el inspector para la distancia de vuelo

    private PlayerMovement movement;
    private PlayerCombat combat;
    private Animator anim;
    private Rigidbody2D rb;

    private bool isStunned = false;
    private float stunTimer = 0f;
    private float currentKnockbackSpeed = 0f;

    void Start()
    {
        currentHealth = maxHealth;

        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (movement == null) Debug.LogError($"[PlayerStats] No se encontró PlayerMovement en {gameObject.name}");
        if (anim == null) Debug.LogError($"[PlayerStats] No se encontró el Animator en los hijos de {gameObject.name}");
        if (rb == null) Debug.LogError($"[PlayerStats] No se encontró Rigidbody2D en {gameObject.name}");
    }

    void Update()
    {
        if (isStunned)
        {
            movement.isAttacking = true;
            stunTimer -= Time.deltaTime;

            // --- CORRECCIÓN EXCLUSIVA PARA EL VUELO ---
            // Modificamos 'rb.position' directamente en lugar de la velocidad.
            // Esto le gana al script de movimiento si este intenta frenar al jugador.
            if (rb != null && currentKnockbackSpeed != 0f)
            {
                rb.position = new Vector2(rb.position.x + currentKnockbackSpeed * Time.deltaTime, rb.position.y);
            }

            // Desaceleración progresiva para que el deslizamiento sea fluido y decreciente
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
        if (movement.isBlocking) return;

        currentHealth -= daño;
        if (currentHealth < 0) currentHealth = 0;

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

    private void AplicarKnockback()
    {
        if (movement.rival != null && rb != null)
        {
            float direccion = transform.position.x > movement.rival.position.x ? 1f : -1f;
            currentKnockbackSpeed = direccion * knockbackForce;
        }
    }
}