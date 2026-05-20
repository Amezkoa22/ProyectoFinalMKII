using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Configuración del Ataque")]
    public int damage = 10;
    public AttackType typeOfAttack;

    private string ownerTag;

    void Start()
    {
        Transform rootParent = transform.root;
        ownerTag = rootParent.tag;

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

    void OnTriggerEnter2D(Collider2D other)
    {
        Hurtbox rivalHurtbox = other.GetComponent<Hurtbox>();

        if (rivalHurtbox == null) return;

        if (other.transform.root.tag == ownerTag)
        {
            return;
        }

        rivalHurtbox.RegistrarImpacto(damage, typeOfAttack);
    }
}