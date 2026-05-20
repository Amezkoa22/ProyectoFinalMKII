using UnityEngine;

public class IceBall : MonoBehaviour
{
    private float speed;
    private float freezeDuration;
    private Transform owner;
    private bool stateHit = false;
    private Animator anim;
    private Rigidbody2D rb;

    public void Inicializar(float velocidad, float duracionCongelado, Transform creador, bool haciaDerecha)
    {
        speed = velocidad;
        freezeDuration = duracionCongelado;
        owner = creador;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        transform.localScale = new Vector3(haciaDerecha ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(haciaDerecha ? speed : -speed, 0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (stateHit) return;

        if (collision.transform != owner && (collision.CompareTag("Player") || collision.GetComponent<PlayerMovement>() != null))
        {
            PlayerMovement rivalMove = collision.GetComponent<PlayerMovement>();

            if (rivalMove != null)
            {
                stateHit = true;
                if (rb != null) rb.linearVelocity = Vector2.zero; // Detener el proyectil inmediatamente

                if (!rivalMove.isBlocking)
                {
                    PlayerCombat rivalCombat = collision.GetComponent<PlayerCombat>();
                    if (rivalCombat != null)
                    {
                        rivalCombat.CongelarPorHielo(freezeDuration);
                    }

                    PlayerStats rivalStats = collision.GetComponent<PlayerStats>();
                    if (rivalStats != null)
                    {
                        rivalStats.RecibirGolpe(5, AttackType.Mid, Hurtbox.HurtboxType.Normal);
                    }
                }
                else
                {
                    PlayerStats rivalStats = collision.GetComponent<PlayerStats>();
                    if (rivalStats != null)
                    {
                        rivalStats.RecibirGolpe(5, AttackType.Mid, Hurtbox.HurtboxType.Normal);
                    }
                }

                ExecuteImpact();
            }
        }
    }

    void ExecuteImpact()
    {
        if (anim != null)
        {
            anim.Play("IceImpact", 0, 0f);
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            Destroy(gameObject, stateInfo.length > 0 ? stateInfo.length : 0.3f);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}