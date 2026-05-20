using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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

    [Header("Referencias UI Letreros")]
    public GameObject uiFight;
    public GameObject uiFinishHim;
    public GameObject uiFinishHer;
    public GameObject uiScorpionWins;
    public GameObject uiSubZeroWins;
    public GameObject uiKitanaWins;

    [Header("Referencias UI Barras de Vida")]
    public Image barraVidaJ1;
    public Image barraVidaJ2;
    public Image imgNombreJ1;
    public Image imgNombreJ2;

    [Header("Sprites de Nombres")]
    public Sprite nombreScorpion;
    public Sprite nombreSubZero;
    public Sprite nombreKitana;

    private GameObject jugador1;
    private GameObject jugador2;
    private PlayerCombat combatJ1, combatJ2;
    private PlayerMovement movJ1, movJ2;
    private PlayerController ctrlJ1, ctrlJ2;
    private Animator animJ1, animJ2;
    private Rigidbody2D rbJ1, rbJ2;

    private bool faseFatalityActiva = false;
    private bool peleaTerminada = false;

    public void IniciarRonda(GameObject j1, GameObject j2)
    {
        jugador1 = j1;
        jugador2 = j2;

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

        AsignarSpritesNombres();
        ResetearBarrasVida();
        CongelarJugadores(true);
        StartCoroutine(RutinaInicioRonda());
    }

    private void AsignarSpritesNombres()
    {
        imgNombreJ1.sprite = ObtenerSpriteNombre(Datos_Partida.personajeJugador1);
        imgNombreJ2.sprite = ObtenerSpriteNombre(Datos_Partida.personajeJugador2);
    }

    private Sprite ObtenerSpriteNombre(TipoPersonaje tipo)
    {
        switch (tipo)
        {
            case TipoPersonaje.Scorpion: return nombreScorpion;
            case TipoPersonaje.SubZero: return nombreSubZero;
            case TipoPersonaje.Kitana: return nombreKitana;
        }
        return null;
    }

    private void ResetearBarrasVida()
    {
        barraVidaJ1.fillAmount = 1f;
        barraVidaJ2.fillAmount = 1f;
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
        int ganador = playerNumber == 1 ? 2 : 1;

        if (ganador == 1) Datos_Partida.victoriasJ1++;
        else Datos_Partida.victoriasJ2++;

        if ((ganador == 1 ? Datos_Partida.victoriasJ1 : Datos_Partida.victoriasJ2) >= 2)
        {
            faseFatalityActiva = true;
            StartCoroutine(RutinaFinishHim(playerNumber));
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
        animPerdedor.Play("Dizzy", 0, 0f);

        if (perdedorNum == 1)
        {
            if (movJ1 != null) movJ1.enabled = false;
            if (ctrlJ1 != null) ctrlJ1.enabled = false;
            if (combatJ1 != null) combatJ1.enabled = false;
            if (rbJ1 != null) rbJ1.linearVelocity = Vector2.zero;
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
            if (movJ1 != null) movJ1.enabled = true;
            if (ctrlJ1 != null) ctrlJ1.enabled = true;
            if (combatJ1 != null) combatJ1.enabled = true;
        }

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
        StartCoroutine(RutinaPantallaVictoria(ganadorNum == 1 ? Datos_Partida.personajeJugador1 : Datos_Partida.personajeJugador2));
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
        bool activar = !estado;
        if (movJ1 != null) movJ1.enabled = activar;
        if (ctrlJ1 != null) ctrlJ1.enabled = activar;
        if (combatJ1 != null) combatJ1.enabled = activar;
        if (movJ2 != null) movJ2.enabled = activar;
        if (ctrlJ2 != null) ctrlJ2.enabled = activar;
        if (combatJ2 != null) combatJ2.enabled = activar;
        if (estado)
        {
            if (rbJ1 != null) rbJ1.linearVelocity = Vector2.zero;
            if (rbJ2 != null) rbJ2.linearVelocity = Vector2.zero;
        }
    }

    public void ActualizarInterfazVida(int playerNumber, float porcentaje)
    {
        if (playerNumber == 1) barraVidaJ1.fillAmount = porcentaje;
        else barraVidaJ2.fillAmount = porcentaje;
    }
}