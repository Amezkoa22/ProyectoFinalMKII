using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Menú de selección de personaje por turnos.
// Turno 1: el Jugador 1 elige; al confirmar, ese personaje queda bloqueado.
// Turno 2: el Jugador 2 elige entre los restantes; al confirmar, se escribe en
// Datos_Partida, se elige una arena aleatoria y se carga la escena de pelea.
public class CharacterSelect : MonoBehaviour
{
    [Header("Botones de personaje (orden: Scorpion, SubZero, Kitana)")]
    public Image[] botones;                    // Imágenes/sprites de cada personaje
    public Image[] bordes;                     // Bordes que indican selección (mismo índice)
    public TipoPersonaje[] personajesPorBoton; // Tipo correspondiente a cada índice

    [Header("Colores del borde animado")]
    public Color colorJugador1 = new Color(1f, 0.2f, 0.2f); // rojo para turno J1
    public Color colorJugador2 = new Color(0.2f, 0.4f, 1f); // azul para turno J2
    public Color colorBordeAlterno = new Color(0f, 0.6f, 0f);

    [Header("Color del botón bloqueado (el que ya eligió J1)")]
    public Color colorBloqueado = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Header("Velocidad del parpadeo del borde")]
    public float velocidadParpadeo = 4f;

    [Header("Escena de pelea a cargar al terminar la selección")]
    public string escenaPelea = "Pelea";

    [Header("Efectos de Sonido Arcade")]
    public AudioClip audioNavegacion;   // Sonido al mover el cursor
    public AudioClip audioConfirmacion;  // Sonido al confirmar la selección
    private AudioSource audioSource;

    private int indiceActual = 0;
    private int turnoActual = 1;       // 1 = Jugador 1, 2 = Jugador 2
    private int indiceJugador1 = -1;   // -1 = J1 aún no ha confirmado
    private Color[] coloresOriginalesBotones;
    private float timer;

    void Start()
    {
        // Guardar el color original de cada botón para poder restaurarlo si hiciera falta
        coloresOriginalesBotones = new Color[botones.Length];
        for (int i = 0; i < botones.Length; i++)
        {
            coloresOriginalesBotones[i] = botones[i].color;
        }

        // Conseguimos o añadimos el componente de audio automáticamente
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        ActualizarBordes();
    }

    void Update()
    {
        ManejarInput();
        AnimarBordeSeleccionado();
    }

    void ManejarInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoverCursor(1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MoverCursor(-1);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            Confirmar();
        }
    }

    // Mueve el cursor y, si en el turno del J2 cae sobre el bloqueado, lo salta
    void MoverCursor(int direccion)
    {
        int intentos = 0;
        do
        {
            indiceActual += direccion;
            if (indiceActual >= botones.Length) indiceActual = 0;
            if (indiceActual < 0) indiceActual = botones.Length - 1;
            intentos++;
        }
        while (turnoActual == 2 && indiceActual == indiceJugador1 && intentos < botones.Length);

        // Sonar navegación al mover el cursor de selección
        ReproducirSonido(audioNavegacion);
        ActualizarBordes();
    }

    // Activa el borde del índice actual, desactiva los demás, y pinta el bloqueado en gris
    void ActualizarBordes()
    {
        for (int i = 0; i < bordes.Length; i++)
        {
            bordes[i].gameObject.SetActive(i == indiceActual);
        }
        for (int i = 0; i < botones.Length; i++)
        {
            if (i == indiceJugador1)
            {
                botones[i].color = colorBloqueado;
            }
        }
    }

    void AnimarBordeSeleccionado()
    {
        timer += Time.deltaTime * velocidadParpadeo;
        Color colorTurno = (turnoActual == 1) ? colorJugador1 : colorJugador2;

        if (Mathf.FloorToInt(timer) % 2 == 0)
        {
            bordes[indiceActual].color = colorTurno;
        }
        else
        {
            bordes[indiceActual].color = colorBordeAlterno;
        }
    }

    void Confirmar()
    {
        if (turnoActual == 1)
        {
            // Sonar confirmación para el Jugador 1
            ReproducirSonido(audioConfirmacion);

            // Bloquear el personaje del J1 y pasar al turno del J2
            indiceJugador1 = indiceActual;
            Datos_Partida.personajeJugador1 = personajesPorBoton[indiceActual];
            turnoActual = 2;

            // Mover el cursor al primer índice disponible (que no sea el bloqueado)
            indiceActual = 0;
            if (indiceActual == indiceJugador1) indiceActual = 1;
            ActualizarBordes();
        }
        else
        {
            // Seguridad: el cursor no debería poder estar sobre el bloqueado, pero por si acaso
            if (indiceActual == indiceJugador1) return;

            // Sonar confirmación para el Jugador 2 (antes de cargar la pelea)
            ReproducirSonido(audioConfirmacion);

            Datos_Partida.personajeJugador2 = personajesPorBoton[indiceActual];

            // Escenario aleatorio entre los tres
            TipoEscenario[] todos = (TipoEscenario[])System.Enum.GetValues(typeof(TipoEscenario));
            Datos_Partida.escenario = todos[Random.Range(0, todos.Length)];

            SceneManager.LoadScene(escenaPelea);
        }
    }

    // Método auxiliar para reproducir efectos de manera limpia
    private void ReproducirSonido(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}