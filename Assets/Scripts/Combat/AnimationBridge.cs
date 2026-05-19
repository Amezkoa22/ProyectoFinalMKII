using UnityEngine;

public class AnimationBridge : MonoBehaviour
{
    private PlayerCombat playerCombat;

    void Start()
    {
        // Busca automáticamente el componente PlayerCombat en el objeto padre
        playerCombat = GetComponentInParent<PlayerCombat>();

        if (playerCombat == null)
        {
            Debug.LogError("No se encontró el componente PlayerCombat en el objeto padre de " + gameObject.name);
        }
    }

    // ==========================================
    // PUENTES PARA COMBATE BÁSICO
    // ==========================================

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

    public void ActivarHitboxLowPunch()
    {
        if (playerCombat != null) playerCombat.ActivarHitboxLowPunch();
    }

    public void DesactivarHitboxLowPunch()
    {
        if (playerCombat != null) playerCombat.DesactivarHitboxLowPunch();
    }

    public void ActivarHitboxUpperCut()
    {
        if (playerCombat != null) playerCombat.ActivarHitboxUpperCut();
    }

    public void DesactivarHitboxUpperCut()
    {
        if (playerCombat != null) playerCombat.DesactivarHitboxUpperCut();
    }

    // ==========================================
    // PUENTE PARA EL ATAQUE ESPECIAL (SPEAR)
    // ==========================================

    public void DispararSpear()
    {
        if (playerCombat != null)
        {
            playerCombat.DispararSpear();
        }
    }
}