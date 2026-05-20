using UnityEngine;
using UnityEngine.SceneManagement;

public class Cargador_Pelea : MonoBehaviour
{
    [Header("Prefabs de personajes")]
    public GameObject prefabScorpion;
    public GameObject prefabSubZero;
    public GameObject prefabKitana;

    [Header("Puntos de spawn (los mismos para todas las arenas)")]
    public Transform spawnJugador1;
    public Transform spawnJugador2;

    [Header("Nombres de las escenas de arena (deben estar en Build Settings)")]
    public string escenaDeadPool = "Dead_Pool";
    public string escenaKansArena = "Khan_Arena";
    public string escenaArmory = "Armory";

    [Header("Referencias del Sistema")]
    public Camara_Seguimiento camara;
    public Manager_Rondas managerRondas;

    void Start()
    {
        GameObject prefabJ1 = ObtenerPrefab(Datos_Partida.personajeJugador1);
        GameObject jugador1 = Instantiate(prefabJ1, spawnJugador1.position, spawnJugador1.rotation);
        jugador1.tag = "Player1";

        GameObject prefabJ2 = ObtenerPrefab(Datos_Partida.personajeJugador2);
        GameObject jugador2 = Instantiate(prefabJ2, spawnJugador2.position, spawnJugador2.rotation);
        jugador2.tag = "Player2";

        AsignarRival(jugador1, jugador2.transform);
        AsignarRival(jugador2, jugador1.transform);

        AsignarNumeroJugador(jugador1, 1);
        AsignarNumeroJugador(jugador2, 2);

        if (camara != null)
        {
            camara.jugador1 = jugador1.transform;
            camara.jugador2 = jugador2.transform;
        }

        SceneManager.sceneLoaded += AlCargarArena;
        string nombreEscena = ObtenerNombreEscenario(Datos_Partida.escenario);
        SceneManager.LoadScene(nombreEscena, LoadSceneMode.Additive);

        if (managerRondas != null)
        {
            managerRondas.IniciarRonda(jugador1, jugador2);
        }
    }

    void AlCargarArena(Scene escenaCargada, LoadSceneMode modo)
    {
        if (modo != LoadSceneMode.Additive) return;

        GameObject[] raices = escenaCargada.GetRootGameObjects();
        foreach (GameObject raiz in raices)
        {
            Camera[] camarasArena = raiz.GetComponentsInChildren<Camera>(true);
            foreach (Camera c in camarasArena)
            {
                c.gameObject.SetActive(false);
            }

            AudioListener[] listenersArena = raiz.GetComponentsInChildren<AudioListener>(true);
            foreach (AudioListener al in listenersArena)
            {
                al.enabled = false;
            }
        }

        SceneManager.sceneLoaded -= AlCargarArena;
    }

    void AsignarRival(GameObject jugador, Transform rival)
    {
        PlayerMovement movimiento = jugador.GetComponentInChildren<PlayerMovement>(true);
        if (movimiento != null) movimiento.rival = rival;

        PlayerController controlador = jugador.GetComponentInChildren<PlayerController>(true);
        if (controlador != null) controlador.rival = rival;
    }

    void AsignarNumeroJugador(GameObject jugador, int numero)
    {
        PlayerMovement movimiento = jugador.GetComponentInChildren<PlayerMovement>(true);
        if (movimiento != null) movimiento.playerNumber = numero;

        PlayerCombat combate = jugador.GetComponentInChildren<PlayerCombat>(true);
        if (combate != null) combate.playerNumber = numero;
    }

    GameObject ObtenerPrefab(TipoPersonaje tipo)
    {
        switch (tipo)
        {
            case TipoPersonaje.Scorpion: return prefabScorpion;
            case TipoPersonaje.SubZero: return prefabSubZero;
            case TipoPersonaje.Kitana: return prefabKitana;
        }
        return null;
    }

    string ObtenerNombreEscenario(TipoEscenario tipo)
    {
        switch (tipo)
        {
            case TipoEscenario.DeadPool: return escenaDeadPool;
            case TipoEscenario.KansArena: return escenaKansArena;
            case TipoEscenario.Armory: return escenaArmory;
        }
        return "";
    }
}