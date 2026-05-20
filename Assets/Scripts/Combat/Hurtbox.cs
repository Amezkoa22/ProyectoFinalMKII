using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    public enum HurtboxType { Normal, Crouch, Jump }

    [Header("Configuración de la Hurtbox")]
    public HurtboxType tipoDeHurtbox;

    private PlayerStats playerStats;

    void Start()
    {
        playerStats = GetComponentInParent<PlayerStats>();

        if (playerStats == null)
        {
            Debug.LogError($"[Hurtbox] No se encontró el componente PlayerStats en la raíz de: {gameObject.name}");
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    public void RegistrarImpacto(int daño, AttackType tipoDeAtaque)
    {
        if (playerStats != null)
        {
            playerStats.RecibirGolpe(daño, tipoDeAtaque, tipoDeHurtbox);
        }
    }
}