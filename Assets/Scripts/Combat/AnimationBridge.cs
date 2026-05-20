using UnityEngine;

public class AnimationBridge : MonoBehaviour
{
    private PlayerCombat playerCombat;

    void Start()
    {
        playerCombat = GetComponentInParent<PlayerCombat>();

        if (playerCombat == null)
        {
            Debug.LogError("No se encontró el componente PlayerCombat en el objeto padre de " + gameObject.name);
        }
    }

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

    public void DispararSpear()
    {
        if (playerCombat != null)
        {
            playerCombat.DispararSpear();
        }
    }

    public void DispararHieloSubZero()
    {
        PlayerCombat combat = GetComponentInParent<PlayerCombat>();
        if (combat == null) combat = GetComponent<PlayerCombat>();

        if (combat != null)
        {
            combat.LanzarIceBall();
        }
    }

    public void FinAnimacionIceBall()
    {
        PlayerCombat combat = GetComponentInParent<PlayerCombat>();
        if (combat == null) combat = GetComponent<PlayerCombat>();

        if (combat != null)
        {
            combat.TerminarHabilidadIceBall();
        }
    }

    public void LanzarFan()
    {
        if (playerCombat != null)
        {
            playerCombat.LanzarFan();
        }
    }

    public void TerminarHabilidadFan()
    {
        if (playerCombat != null)
        {
            playerCombat.TerminarHabilidadFan();
        }
    }
}