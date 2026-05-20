using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MenuPausa : MonoBehaviour
{
    [Header("UI Componentes")]
    public GameObject panelPausa;
    public TextMeshProUGUI[] textosOpciones; // Elemento 0 = Resume, Elemento 1 = Main Menu

    [Header("Paleta de Color Arcade (Seleccionado)")]
    public Color verdeClaro = new Color(0f, 1f, 0.5f); // Verde neón arcade
    public Color azulOscuro = new Color(0f, 0.1f, 0.5f); // Azul profundo arcade
    public float velocidadParpadeoSeleccion = 15f;

    [Header("Paleta de Color Confirmación (Enter)")]
    public Color blanco = Color.white;
    public Color amarillo = Color.yellow;
    public float duracionParpadeoPresionado = 0.4f;

    private int indiceSeleccionado = 0;
    private bool juegoPausado = false;
    private bool ejecutandoAccion = false;

    void Start()
    {
        // El juego siempre inicia despausado
        panelPausa.SetActive(false);
        juegoPausado = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Si el jugador presionó Enter y se está ejecutando la animación de carga, bloqueamos inputs del menú
        if (ejecutandoAccion) return;

        // Activar / Desactivar Pausa con ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
                DespausarJuego();
            else
                PausarJuego();
        }

        // Lógica de navegación mediante teclado cuando está pausado
        if (juegoPausado)
        {
            ManejarNavegacionTeclado();
            EfectoParpadeoSeleccionado();
        }
    }

    void PausarJuego()
    {
        juegoPausado = true;
        panelPausa.SetActive(true);
        Time.timeScale = 0f; // <--- CONGELA ABSOLUTAMENTE TODO EL JUEGO

        indiceSeleccionado = 0;
        RestablecerEstiloTextos();
    }

    public void DespausarJuego()
    {
        juegoPausado = false;
        panelPausa.SetActive(false);
        Time.timeScale = 1f; // <--- EL TIEMPO VUELVE A LA NORMALIDAD
    }

    void ManejarNavegacionTeclado()
    {
        // Flecha Arriba
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            indiceSeleccionado--;
            if (indiceSeleccionado < 0) indiceSeleccionado = textosOpciones.Length - 1;
            RestablecerEstiloTextos();
        }
        // Flecha Abajo
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            indiceSeleccionado++;
            if (indiceSeleccionado >= textosOpciones.Length) indiceSeleccionado = 0;
            RestablecerEstiloTextos();
        }

        // Confirmar con Enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(SecuenciaConfirmacionArcade());
        }
    }

    void RestablecerEstiloTextos()
    {
        for (int i = 0; i < textosOpciones.Length; i++)
        {
            if (i != indiceSeleccionado)
            {
                // Las opciones no seleccionadas se quedan de un color gris/blanco apagado estático
                textosOpciones[i].color = new Color(0.7f, 0.7f, 0.7f);
            }
        }
    }

    void EfectoParpadeoSeleccionado()
    {
        // Usamos Time.unscaledTime porque Time.time vale 0 cuando el juego está pausado
        float lerp = Mathf.PingPong(Time.unscaledTime * velocidadParpadeoSeleccion, 1f);
        textosOpciones[indiceSeleccionado].color = Color.Lerp(verdeClaro, azulOscuro, lerp);
    }

    IEnumerator SecuenciaConfirmacionArcade()
    {
        ejecutandoAccion = true;
        float tiempoTranscurrido = 0f;
        bool colorAlternante = false;

        // Parpadeo ultra rápido entre Blanco y Amarillo al estilo "INSERT COIN"
        while (tiempoTranscurrido < duracionParpadeoPresionado)
        {
            textosOpciones[indiceSeleccionado].color = colorAlternante ? blanco : amarillo;
            colorAlternante = !colorAlternante;

            float intervalo = 0.04f; // velocidad del destello de confirmación
            tiempoTranscurrido += intervalo;

            // Requerimos "Realtime" para ignorar por completo el Time.timeScale = 0
            yield return new WaitForSecondsRealtime(intervalo);
        }

        // Ejecutar las acciones tras el parpadeo
        if (indiceSeleccionado == 0)
        {
            // Opción: RESUME
            DespausarJuego();
            ejecutandoAccion = false;
        }
        else if (indiceSeleccionado == 1)
        {
            // Opción: BACK TO MAIN MENU
            Time.timeScale = 1f; // ¡REGLA DE ORO! Siempre devuelve el tiempo a 1 antes de cambiar de escena
            SceneManager.LoadScene("Menu_inicio");
        }
    }
}