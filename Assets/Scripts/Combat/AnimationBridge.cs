using UnityEngine;

public class AnimationBridge : MonoBehaviour
{
    private PlayerCombat playerCombat;

    void Start()
    {
        // Buscamos el componente PlayerCombat en los objetos padres (la raíz del personaje)
        playerCombat = GetComponentInParent<PlayerCombat>();

        if (playerCombat == null)
        {
            Debug.LogError($"[AnimationBridge] No se encontró el script PlayerCombat en los padres de {gameObject.name}. Asegúrate de que este objeto sea hijo de la raíz del jugador.");
        }
    }

    // --- ESTAS FUNCIONES APARECERÁN DE INMEDIATO EN TU DESPLEGABLE DE ANIMACIÓN ---

    public void ActivarHitboxPunch()
    {
        if (playerCombat != null) playerCombat.ActivarHitboxPunch();
    }

    public void DesactivarHitboxPunch()
    {
        if (playerCombat != null) playerCombat.DesactivarHitboxPunch();
    }

    public void ActivarHitboxKick()
    {
        if (playerCombat != null) playerCombat.ActivarHitboxKick();
    }

    public void DesactivarHitboxKick()
    {
        if (playerCombat != null) playerCombat.DesactivarHitboxKick();
    }

    public void ActivarHitboxUpperCut()
    {
        if (playerCombat != null) playerCombat.ActivarHitboxUpperCut();
    }

    public void DesactivarHitboxUpperCut()
    {
        if (playerCombat != null) playerCombat.DesactivarHitboxUpperCut();
    }
}