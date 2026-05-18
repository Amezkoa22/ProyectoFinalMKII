using UnityEngine;

public class Camara_Seguimiento : MonoBehaviour
{
    public Transform jugador1;
    public Transform jugador2;

    public float alturaFija = 0f;
    public float suavizado = 5f;

    public float limiteIzquierdo = -10f;
    public float limiteDerecho = 10f;

    public float margenJugador = 0.5f;

    private Camera camara;

    void Start()
    {
        camara = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (jugador1 == null || jugador2 == null) return;

        float puntoMedioX = (jugador1.position.x + jugador2.position.x) / 2f;

        puntoMedioX = Mathf.Clamp(puntoMedioX, limiteIzquierdo, limiteDerecho);

        Vector3 posicionObjetivo = new Vector3(puntoMedioX, alturaFija, transform.position.z);

        transform.position = Vector3.Lerp(transform.position, posicionObjetivo, suavizado * Time.deltaTime);

        LimitarJugadores();
    }

    void LimitarJugadores()
    {
        float mitadAlto = camara.orthographicSize;
        float mitadAncho = mitadAlto * camara.aspect;

        float bordeIzquierdo = transform.position.x - mitadAncho + margenJugador;
        float bordeDerecho = transform.position.x + mitadAncho - margenJugador;

        Vector3 pos1 = jugador1.position;
        pos1.x = Mathf.Clamp(pos1.x, bordeIzquierdo, bordeDerecho);
        jugador1.position = pos1;

        Vector3 pos2 = jugador2.position;
        pos2.x = Mathf.Clamp(pos2.x, bordeIzquierdo, bordeDerecho);
        jugador2.position = pos2;
    }
}