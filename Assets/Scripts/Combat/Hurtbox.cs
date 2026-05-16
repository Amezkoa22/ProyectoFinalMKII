using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    public enum HurtboxType { Normal, Crouch, Jump }

    [Header("Configuración de la Hurtbox")]
    public HurtboxType tipoDeHurtbox;

    // Conexión con el script central
    private PlayerStats playerStats;

    void Start()
    {
        // Buscamos el componente central PlayerStats en la raíz del personaje principal
        playerStats = GetComponentInParent<PlayerStats>();

        if (playerStats == null)
        {
            Debug.LogError($"[Hurtbox] No se encontró el componente PlayerStats en la raíz de: {gameObject.name}");
        }

        // Forzamos que sea Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    // Este método será invocado públicamente por la Hitbox atacante del rival al hacer contacto
    public void RegistrarImpacto(int daño, AttackType tipoDeAtaque)
    {
        if (playerStats != null)
        {
            // Enviamos los datos exactos del golpe recibido al gestor central del jugador
            playerStats.RecibirGolpe(daño, tipoDeAtaque, tipoDeHurtbox);
        }
    }
}