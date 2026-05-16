using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 100;
    public int currentHealth;

    // Referencias lógicas a tus scripts existentes en el mismo objeto
    private PlayerMovement movement;
    private Animator anim;

    void Start()
    {
        currentHealth = maxHealth;

        // Obtenemos los componentes directamente de la raíz del personaje
        movement = GetComponent<PlayerMovement>();
        anim = GetComponentInChildren<Animator>();

        if (movement == null) Debug.LogError($"[PlayerStats] No se encontró PlayerMovement en {gameObject.name}");
        if (anim == null) Debug.LogError($"[PlayerStats] No se encontró el Animator en los hijos de {gameObject.name}");
    }

    // Este método será invocado públicamente desde el script Hurtbox cuando detecte un golpe
    public void RecibirGolpe(int daño, AttackType tipoDeAtaque, Hurtbox.HurtboxType hurtboxImpactada)
    {
        // 1. REVISAR SI EL JUGADOR ESTÁ BLOQUEANDO
        if (movement.isBlocking)
        {
            // Si está bloqueando, no aplicamos daño ni reproducimos animaciones de Hit.
            // El flujo se interrumpe aquí para mantener al jugador en sus estados de bloqueo actuales.
            return;
        }

        // 2. APLICAR DAÑO (Si no está bloqueando)
        currentHealth -= daño;
        if (currentHealth < 0) currentHealth = 0;

        // 3. FILTRADO ESTRICTO DE ANIMACIONES DE IMPACTO (Tus reglas exactas)

        // REGLA A: Si se impacta la Hurtbox de Salto (Jump), SIEMPRE va a Hit_UpperCut
        if (hurtboxImpactada == Hurtbox.HurtboxType.Jump)
        {
            anim.Play("Hit_UpperCut", 0, 0f);
        }
        // REGLA B: Si se impacta la Hurtbox Agachado (Crouch), SIEMPRE va a Hit_Duck
        else if (hurtboxImpactada == Hurtbox.HurtboxType.Crouch)
        {
            anim.Play("Hit_Duck", 0, 0f);
        }
        // REGLA C: Si se impacta la Hurtbox de Pie (Normal)
        else if (hurtboxImpactada == Hurtbox.HurtboxType.Normal)
        {
            // Sub-regla: Si el golpe recibido fue un UpperCut pesado, va a Hit_UpperCut
            if (tipoDeAtaque == AttackType.Heavy)
            {
                anim.Play("Hit_UpperCut", 0, 0f);
            }
            // Para cualquier otro golpe estándar de pie, va a Hit_Stand
            else
            {
                anim.Play("Hit_Stand", 0, 0f);
            }
        }
    }
}