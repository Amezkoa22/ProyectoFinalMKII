using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform rival; // Aquí arrastraremos al otro jugador en el editor
    public Transform visualPart; // Aquí arrastraremos el objeto hijo "Sprite"

    void Update()
    {
        LookAtRival();
    }

    void LookAtRival()
    {
        if (rival != null)
        {
            // Comparamos posiciones X
            if (rival.position.x > transform.position.x)
            {
                // El rival está a la derecha, mirar a la derecha
                visualPart.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                // El rival está a la izquierda, mirar a la izquierda (espejo)
                visualPart.localScale = new Vector3(-1, 1, 1);
            }
        }
    }
}