using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuMortalKombat : MonoBehaviour
{
    [Header("Selector PNG")]
    public RectTransform flechaTransform;
    public Image flechaImage;

    [Header("Ajustes de Movimiento")]
    [Tooltip("Cuántos píxeles baja la flecha entre cada opción (Prueba con valores entre 30 y 50)")]
    public float espaciadoY = 40f;

    [Header("Nombres de las Escenas (Inspector)")]
    [Tooltip("Escribe aquí el nombre exacto de tu escena de pelea")]
    public string escenaStart;
    [Tooltip("Escribe aquí el nombre exacto de tu escena de créditos")]
    public string escenaCredits;

    [Header("Ajustes Visuales")]
    public float velocidadParpadeo = 0.35f;

    [Header("Efectos de Sonido Arcade")]
    public AudioClip audioNavegacion;   // Sonido al mover la flecha o usar mouse
    public AudioClip audioConfirmacion;  // Sonido al presionar Enter/Espacio
    private AudioSource audioSource;

    private int opcionSeleccionada = 0;
    private float tiempoParpadeo;
    private Vector2 posicionInicialFlecha;

    void Start()
    {
        posicionInicialFlecha = flechaTransform.anchoredPosition;

        // Conseguimos o añadimos el componente de audio automáticamente en el menú
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        ActualizarPosicionFlecha();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            opcionSeleccionada--;
            if (opcionSeleccionada < 0) opcionSeleccionada = 2;

            // Sonar navegación al mover teclado
            ReproducirSonido(audioNavegacion);
            ActualizarPosicionFlecha();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            opcionSeleccionada++;
            if (opcionSeleccionada > 2) opcionSeleccionada = 0;

            // Sonar navegación al mover teclado
            ReproducirSonido(audioNavegacion);
            ActualizarPosicionFlecha();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            // Sonar confirmación al presionar
            ReproducirSonido(audioConfirmacion);
            EjecutarOpcion();
        }

        tiempoParpadeo += Time.deltaTime;
        if (tiempoParpadeo >= velocidadParpadeo)
        {
            flechaImage.enabled = !flechaImage.enabled;
            tiempoParpadeo = 0;
        }
    }

    void ActualizarPosicionFlecha()
    {
        flechaImage.enabled = true;
        tiempoParpadeo = 0;
        float nuevaY = posicionInicialFlecha.y - (opcionSeleccionada * espaciadoY);

        flechaTransform.anchoredPosition = new Vector2(posicionInicialFlecha.x, nuevaY);
    }

    public void SeleccionarOpcionPorMouse(int indice)
    {
        // Solo reproducimos el sonido si el mouse realmente cambia de opción (evita spam de clics)
        if (opcionSeleccionada != indice)
        {
            opcionSeleccionada = indice;
            ReproducirSonido(audioNavegacion);
            ActualizarPosicionFlecha();
        }
    }

    // Método auxiliar para reproducir los efectos de manera segura
    private void ReproducirSonido(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void EjecutarOpcion()
    {
        switch (opcionSeleccionada)
        {
            case 0:
                if (!string.IsNullOrEmpty(escenaStart))
                {
                    SceneManager.LoadScene(escenaStart);
                }
                else
                {
                    Debug.LogWarning("Pon el nombre de la escena :/");
                }
                break;

            case 1:
                Debug.Log("Saliendo...");
                Application.Quit();
                break;

            case 2:
                if (!string.IsNullOrEmpty(escenaCredits))
                {
                    SceneManager.LoadScene(escenaCredits);
                }
                else
                {
                    Debug.Log("Mostrando Créditos.");
                }
                break;
        }
    }
}