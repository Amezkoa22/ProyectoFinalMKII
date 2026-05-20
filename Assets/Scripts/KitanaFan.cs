using UnityEngine;

public class KitanaFan : MonoBehaviour
{
    private float speed;
    private Transform owner;
    private bool moveRight;
    private Rigidbody2D rb;

    public void Inicializar(float velocidad, Transform creador, bool haciaDerecha)
    {
        speed = velocidad;
        owner = creador;
        moveRight = haciaDerecha;

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(moveRight ? speed : -speed, 0f);
        }

        transform.localScale = new Vector3(moveRight ? 1f : -1f, 1f, 1f);

        Destroy(gameObject, 4f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (owner != null && (collision.transform == owner || collision.transform.IsChildOf(owner)))
            return;

        PlayerStats rivalStats = collision.GetComponent<PlayerStats>();

        if (rivalStats != null)
        {
            rivalStats.RecibirGolpe(12, AttackType.Mid, Hurtbox.HurtboxType.Normal);

            Destroy(gameObject);
        }
        else if (collision.CompareTag("Escenario") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}