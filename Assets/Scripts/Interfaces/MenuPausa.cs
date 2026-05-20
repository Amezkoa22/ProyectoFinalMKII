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
    public Color verdeClaro = new Color(0f, 1f, 0.5f);
    public Color azulOscuro = new Color(0f, 0.1f, 0.5f);
    public float velocidadParpadeoSeleccion = 15f;

    [Header("Paleta de Color Confirmación (Enter)")]
    public Color blanco = Color.white;
    public Color amarillo = Color.yellow;
    public float duracionParpadeoPresionado = 0.4f;

    [Header("Efectos de Sonido Arcade")]
    public AudioClip audioNavegacion;   // Sonido al mover las flechas
    public AudioClip audioConfirmacion;  // Sonido al presionar Enter
    private AudioSource audioSource;

    private int indiceSeleccionado = 0;
    private bool juegoPausado = false;
    private bool ejecutandoAccion = false;

    void Start()
    {
        panelPausa.SetActive(false);
        juegoPausado = false;
        Time.timeScale = 1f;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.ignoreListenerPause = true;
    }

    void Update()
    {
        if (ejecutandoAccion) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
                DespausarJuego();
            else
                PausarJuego();
        }

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
        Time.timeScale = 0f;

        indiceSeleccionado = 0;
        RestablecerEstiloTextos();
    }

    public void DespausarJuego()
    {
        juegoPausado = false;
        panelPausa.SetActive(false);
        Time.timeScale = 1f;
    }

    void ManejarNavegacionTeclado()
    {
        bool seMovio = false;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            indiceSeleccionado--;
            if (indiceSeleccionado < 0) indiceSeleccionado = textosOpciones.Length - 1;
            RestablecerEstiloTextos();
            seMovio = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            indiceSeleccionado++;
            if (indiceSeleccionado >= textosOpciones.Length) indiceSeleccionado = 0;
            RestablecerEstiloTextos();
            seMovio = true;
        }

        if (seMovio && audioSource != null && audioNavegacion != null)
        {
            audioSource.PlayOneShot(audioNavegacion);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (audioSource != null && audioConfirmacion != null)
            {
                audioSource.PlayOneShot(audioConfirmacion);
            }

            StartCoroutine(SecuenciaConfirmacionArcade());
        }
    }

    void RestablecerEstiloTextos()
    {
        for (int i = 0; i < textosOpciones.Length; i++)
        {
            if (i != indiceSeleccionado)
            {
                textosOpciones[i].color = new Color(0.7f, 0.7f, 0.7f);
            }
        }
    }

    void EfectoParpadeoSeleccionado()
    {
        float lerp = Mathf.PingPong(Time.unscaledTime * velocidadParpadeoSeleccion, 1f);
        textosOpciones[indiceSeleccionado].color = Color.Lerp(verdeClaro, azulOscuro, lerp);
    }

    IEnumerator SecuenciaConfirmacionArcade()
    {
        ejecutandoAccion = true;
        float tiempoTranscurrido = 0f;
        bool colorAlternante = false;

        while (tiempoTranscurrido < duracionParpadeoPresionado)
        {
            textosOpciones[indiceSeleccionado].color = colorAlternante ? blanco : amarillo;
            colorAlternante = !colorAlternante;

            float intervalo = 0.04f;
            tiempoTranscurrido += intervalo;

            yield return new WaitForSecondsRealtime(intervalo);
        }

        if (indiceSeleccionado == 0)
        {
            DespausarJuego();
            ejecutandoAccion = false;
        }
        else if (indiceSeleccionado == 1)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Menu_inicio");
        }
    }
}