using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager_Rondas : MonoBehaviour
{
    [Header("Referencias de Audio")]
    public AudioSource audioSource;
    public AudioClip clipRound1;
    public AudioClip clipRound2;
    public AudioClip clipRound3;
    public AudioClip clipFight;
    public AudioClip clipFinishHim;
    public AudioClip clipFinishHer;
    public AudioClip clipScorpionWins;
    public AudioClip clipSubZeroWins;
    public AudioClip clipKitanaWins;

    [Header("Referencias UI (GameObjects)")]
    public GameObject uiFight;
    public GameObject uiFinishHim;
    public GameObject uiFinishHer;
    public GameObject uiScorpionWins;
    public GameObject uiSubZeroWins;
    public GameObject uiKitanaWins;

    private GameObject jugador1;
    private GameObject jugador2;

    // Componentes del Jugador 1
    private PlayerCombat combatJ1;
    private PlayerMovement movJ1;
    private PlayerController ctrlJ1;
    private Animator animJ1;
    private Rigidbody2D rbJ1;

    // Componentes del Jugador 2
    private PlayerCombat combatJ2;
    private PlayerMovement movJ2;
    private PlayerController ctrlJ2;
    private Animator animJ2;
    private Rigidbody2D rbJ2;

    private bool faseFatalityActiva = false;
    private bool peleaTerminada = false;

    public void IniciarRonda(GameObject j1, GameObject j2)
    {
        jugador1 = j1;
        jugador2 = j2;

        // Capturamos todos los scripts clave de ambos personajes
        combatJ1 = jugador1.GetComponentInChildren<PlayerCombat>(true);
        movJ1 = jugador1.GetComponentInChildren<PlayerMovement>(true);
        ctrlJ1 = jugador1.GetComponentInChildren<PlayerController>(true);
        animJ1 = jugador1.GetComponentInChildren<Animator>(true);
        rbJ1 = jugador1.GetComponent<Rigidbody2D>();

        combatJ2 = jugador2.GetComponentInChildren<PlayerCombat>(true);
        movJ2 = jugador2.GetComponentInChildren<PlayerMovement>(true);
        ctrlJ2 = jugador2.GetComponentInChildren<PlayerController>(true);
        animJ2 = jugador2.GetComponentInChildren<Animator>(true);
        rbJ2 = jugador2.GetComponent<Rigidbody2D>();

        // Congelamos todo al iniciar
        CongelarJugadores(true);
        StartCoroutine(RutinaInicioRonda());
    }

    private IEnumerator RutinaInicioRonda()
    {
        yield return new WaitForSeconds(1f);

        if (Datos_Partida.rondaActual == 1 && clipRound1 != null) audioSource.PlayOneShot(clipRound1);
        else if (Datos_Partida.rondaActual == 2 && clipRound2 != null) audioSource.PlayOneShot(clipRound2);
        else if (Datos_Partida.rondaActual == 3 && clipRound3 != null) audioSource.PlayOneShot(clipRound3);

        yield return new WaitForSeconds(1.5f);

        if (clipFight != null) audioSource.PlayOneShot(clipFight);
        if (uiFight != null)
        {
            uiFight.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            uiFight.SetActive(false);
        }

        // Se descongela y se activan los controles para pelear
        CongelarJugadores(false);
    }

    public void ReportarMuerte(int playerNumber)
    {
        if (peleaTerminada || faseFatalityActiva) return;

        CongelarJugadores(true);

        int perdedor = playerNumber;
        int ganador = perdedor == 1 ? 2 : 1;

        if (ganador == 1) Datos_Partida.victoriasJ1++;
        else Datos_Partida.victoriasJ2++;

        int victoriasGanador = ganador == 1 ? Datos_Partida.victoriasJ1 : Datos_Partida.victoriasJ2;

        if (victoriasGanador >= 2)
        {
            faseFatalityActiva = true;
            StartCoroutine(RutinaFinishHim(perdedor));
        }
        else
        {
            StartCoroutine(RutinaSiguienteRonda());
        }
    }

    private IEnumerator RutinaSiguienteRonda()
    {
        yield return new WaitForSeconds(3f);
        Datos_Partida.rondaActual++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator RutinaFinishHim(int perdedorNum)
    {
        yield return new WaitForSeconds(2f);

        TipoPersonaje perdedorTipo = perdedorNum == 1 ? Datos_Partida.personajeJugador1 : Datos_Partida.personajeJugador2;
        Animator animPerdedor = perdedorNum == 1 ? animJ1 : animJ2;

        // Forzar animación de mareo
        animPerdedor.Play("Dizzy", 0, 0f);

        // BLOQUEO ABSOLUTO: Desactivamos por completo los controles del perdedor
        if (perdedorNum == 1)
        {
            if (movJ1 != null) movJ1.enabled = false;
            if (ctrlJ1 != null) ctrlJ1.enabled = false;
            if (combatJ1 != null) combatJ1.enabled = false;
            if (rbJ1 != null) rbJ1.linearVelocity = Vector2.zero;

            // Nos aseguramos de que el ganador (J2) SÍ se pueda mover libremente para el golpe final
            if (movJ2 != null) movJ2.enabled = true;
            if (ctrlJ2 != null) ctrlJ2.enabled = true;
            if (combatJ2 != null) combatJ2.enabled = true;
        }
        else
        {
            if (movJ2 != null) movJ2.enabled = false;
            if (ctrlJ2 != null) ctrlJ2.enabled = false;
            if (combatJ2 != null) combatJ2.enabled = false;
            if (rbJ2 != null) rbJ2.linearVelocity = Vector2.zero;

            // Nos aseguramos de que el ganador (J1) SÍ se pueda mover libremente para el golpe final
            if (movJ1 != null) movJ1.enabled = true;
            if (ctrlJ1 != null) ctrlJ1.enabled = true;
            if (combatJ1 != null) combatJ1.enabled = true;
        }

        // Mostrar Letrero Finish Him / Her
        if (perdedorTipo == TipoPersonaje.Kitana)
        {
            if (clipFinishHer != null) audioSource.PlayOneShot(clipFinishHer);
            if (uiFinishHer != null) uiFinishHer.SetActive(true);
        }
        else
        {
            if (clipFinishHim != null) audioSource.PlayOneShot(clipFinishHim);
            if (uiFinishHim != null) uiFinishHim.SetActive(true);
        }
    }

    public void ReportarGolpeFatality(int perdedorNum)
    {
        if (!faseFatalityActiva || peleaTerminada) return;
        peleaTerminada = true;

        // Ocultar letreros de la fase de ejecución
        if (uiFinishHim != null) uiFinishHim.SetActive(false);
        if (uiFinishHer != null) uiFinishHer.SetActive(false);

        // Congelamos a ambos permanentemente al terminar
        CongelarJugadores(true);

        int ganadorNum = perdedorNum == 1 ? 2 : 1;
        TipoPersonaje ganadorTipo = ganadorNum == 1 ? Datos_Partida.personajeJugador1 : Datos_Partida.personajeJugador2;

        StartCoroutine(RutinaPantallaVictoria(ganadorTipo));
    }

    private IEnumerator RutinaPantallaVictoria(TipoPersonaje ganadorTipo)
    {
        yield return new WaitForSeconds(2f);

        if (ganadorTipo == TipoPersonaje.Scorpion)
        {
            if (clipScorpionWins != null) audioSource.PlayOneShot(clipScorpionWins);
            if (uiScorpionWins != null) uiScorpionWins.SetActive(true);
        }
        else if (ganadorTipo == TipoPersonaje.SubZero)
        {
            if (clipSubZeroWins != null) audioSource.PlayOneShot(clipSubZeroWins);
            if (uiSubZeroWins != null) uiSubZeroWins.SetActive(true);
        }
        else if (ganadorTipo == TipoPersonaje.Kitana)
        {
            if (clipKitanaWins != null) audioSource.PlayOneShot(clipKitanaWins);
            if (uiKitanaWins != null) uiKitanaWins.SetActive(true);
        }
    }

    private void CongelarJugadores(bool estado)
    {
        // Si 'estado' es true, desactivamos los scripts (activarComponentes = false)
        bool activarComponentes = !estado;

        // Jugador 1
        if (movJ1 != null) movJ1.enabled = activarComponentes;
        if (ctrlJ1 != null) ctrlJ1.enabled = activarComponentes;
        if (combatJ1 != null) combatJ1.enabled = activarComponentes;

        // Jugador 2
        if (movJ2 != null) movJ2.enabled = activarComponentes;
        if (ctrlJ2 != null) ctrlJ2.enabled = activarComponentes;
        if (combatJ2 != null) combatJ2.enabled = activarComponentes;

        // Si los estamos congelando, forzamos que sus velocidades físicas vayan a 0 de inmediato
        if (estado)
        {
            if (rbJ1 != null) rbJ1.linearVelocity = Vector2.zero;
            if (rbJ2 != null) rbJ2.linearVelocity = Vector2.zero;
        }
    }
}