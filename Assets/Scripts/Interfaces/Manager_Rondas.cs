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
    private PlayerCombat combatJ1;
    private PlayerCombat combatJ2;
    private Animator animJ1;
    private Animator animJ2;

    private bool faseFatalityActiva = false;
    private bool peleaTerminada = false;

    public void IniciarRonda(GameObject j1, GameObject j2)
    {
        jugador1 = j1;
        jugador2 = j2;

        combatJ1 = jugador1.GetComponentInChildren<PlayerCombat>(true);
        combatJ2 = jugador2.GetComponentInChildren<PlayerCombat>(true);
        animJ1 = jugador1.GetComponentInChildren<Animator>(true);
        animJ2 = jugador2.GetComponentInChildren<Animator>(true);

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
        PlayerCombat combatGanador = perdedorNum == 1 ? combatJ2 : combatJ1;

        animPerdedor.Play("Dizzy", 0, 0f);
        combatGanador.isFrozen = false;

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

        if (uiFinishHim != null) uiFinishHim.SetActive(false);
        if (uiFinishHer != null) uiFinishHer.SetActive(false);

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
        if (combatJ1 != null) combatJ1.isFrozen = estado;
        if (combatJ2 != null) combatJ2.isFrozen = estado;
    }
}