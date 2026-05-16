using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Configuración del Ataque")]
    public int damage = 10;
    public AttackType typeOfAttack;

    // Almacenamos el tag del dueño de esta hitbox para evitar golpearse a sí mismo
    private string ownerTag;

    void Start()
    {
        // Guardamos el tag del objeto raíz para saber de quién es este golpe (ej. "Player1" o "Player2")
        // Como las hitboxes suelen ir dentro de las manos/pies en la jerarquía visual, buscamos en los padres principales.
        Transform rootParent = transform.root;
        ownerTag = rootParent.tag;

        // Forzamos por código que el Collider2D de esta hitbox actúe siempre como un Trigger para registrar la superposición física
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError($"[Hitbox] El objeto {gameObject.name} necesita un Collider2D asignado en el Inspector para registrar colisiones.");
        }
    }

    // Este método de Unity detecta de forma automática cuando este trigger choca contra otro colisionador
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. FILTRADO: Intentamos obtener el script Hurtbox del objeto con el que chocamos
        Hurtbox rivalHurtbox = other.GetComponent<Hurtbox>();

        // Si el objeto no tiene el componente Hurtbox, no nos interesa interactuar con él (puede ser el suelo o una pared)
        if (rivalHurtbox == null) return;

        // 2. SEGURIDAD: Comprobamos si la hurtbox tocada pertenece al mismo personaje que lanzó el ataque
        if (other.transform.root.tag == ownerTag)
        {
            // Si el tag coincide, significa que es nuestro propio cuerpo, por lo que salimos del método para no autoflagelarnos
            return;
        }

        // 3. IMPACTO EXITOSO: Al pasar los filtros, invocamos el método público de la hurtbox rival enviando el daño y el tipo de ataque exacto
        rivalHurtbox.RegistrarImpacto(damage, typeOfAttack);
    }
}