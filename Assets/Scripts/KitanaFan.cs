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
            // Asigna velocidad constante en el eje X
            rb.linearVelocity = new Vector2(moveRight ? speed : -speed, 0f);
        }

        // Voltea el sprite horizontalmente según la dirección a la que viaja
        transform.localScale = new Vector3(moveRight ? 1f : -1f, 1f, 1f);

        // Destrucción de seguridad: si no golpea nada en 4 segundos, se borra solo
        Destroy(gameObject, 4f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Regla de Oro: Evitar que el abanico golpee a la propia Kitana que lo lanzó
        if (owner != null && (collision.transform == owner || collision.transform.IsChildOf(owner)))
            return;

        // Intentamos obtener los componentes de daño del rival
        PlayerStats rivalStats = collision.GetComponent<PlayerStats>();

        if (rivalStats != null)
        {
            // APLICA DAÑO: 12 puntos, Tipo de ataque MID, Hurtbox Normal
            rivalStats.RecibirGolpe(12, AttackType.Mid, Hurtbox.HurtboxType.Normal);

            // DESAPARECE AL GOLPEAR: El abanico se destruye tras cumplir su cometido
            Destroy(gameObject);
        }
        // Si choca contra una pared lateral o suelo que tenga tag o capa de escenario
        else if (collision.CompareTag("Escenario") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}