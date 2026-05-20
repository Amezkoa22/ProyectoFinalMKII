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
            if (rival.position.x > transform.position.x)
            {
                visualPart.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                visualPart.localScale = new Vector3(-1, 1, 1);
            }
        }
    }
}