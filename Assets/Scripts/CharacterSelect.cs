using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class CharacterSelect : MonoBehaviour
{
    [Header("Botones de personaje (orden: Scorpion, SubZero, Kitana)")]
    public Image[] botones;                   
    public Image[] bordes;                     
    public TipoPersonaje[] personajesPorBoton; 

    [Header("Colores del borde animado")]
    public Color colorJugador1 = new Color(1f, 0.2f, 0.2f); 
    public Color colorJugador2 = new Color(0.2f, 0.4f, 1f); 
    public Color colorBordeAlterno = new Color(0f, 0.6f, 0f);

    [Header("Color del botón bloqueado (el que ya eligió J1)")]
    public Color colorBloqueado = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Header("Velocidad del parpadeo del borde")]
    public float velocidadParpadeo = 4f;

    [Header("Escena de pelea a cargar al terminar la selección")]
    public string escenaPelea = "Pelea";

    [Header("Efectos de Sonido Arcade")]
    public AudioClip audioNavegacion;  
    public AudioClip audioConfirmacion;  
    private AudioSource audioSource;

    private int indiceActual = 0;
    private int turnoActual = 1;      
    private int indiceJugador1 = -1;  
    private Color[] coloresOriginalesBotones;
    private float timer;

    void Start()
    {
        coloresOriginalesBotones = new Color[botones.Length];
        for (int i = 0; i < botones.Length; i++)
        {
            coloresOriginalesBotones[i] = botones[i].color;
        }

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

        ReproducirSonido(audioNavegacion);
        ActualizarBordes();
    }

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
            ReproducirSonido(audioConfirmacion);

            indiceJugador1 = indiceActual;
            Datos_Partida.personajeJugador1 = personajesPorBoton[indiceActual];
            turnoActual = 2;

            indiceActual = 0;
            if (indiceActual == indiceJugador1) indiceActual = 1;
            ActualizarBordes();
        }
        else
        {
            if (indiceActual == indiceJugador1) return;

            ReproducirSonido(audioConfirmacion);

            Datos_Partida.personajeJugador2 = personajesPorBoton[indiceActual];

            TipoEscenario[] todos = (TipoEscenario[])System.Enum.GetValues(typeof(TipoEscenario));
            Datos_Partida.escenario = todos[Random.Range(0, todos.Length)];

            SceneManager.LoadScene(escenaPelea);
        }
    }

    private void ReproducirSonido(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}