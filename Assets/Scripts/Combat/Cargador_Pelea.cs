using UnityEngine;
using UnityEngine.SceneManagement;

// Manager de la escena Pelea. Lee Datos_Partida y arma la pelea:
// instancia personajes, asigna sus Transforms a la cámara, configura los
// rivales cruzados y el número de jugador, y carga la arena elegida de forma
// aditiva.
// Al cargar la arena aditivamente, desactiva automáticamente las cámaras y
// AudioListeners propios de la arena para evitar conflictos con la cámara de
// la escena Pelea. Así las arenas siguen siendo jugables standalone para
// pruebas, pero no estorban cuando se cargan dentro del flujo de pelea.

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

    [Header("Referencia a la cámara que sigue a los jugadores")]
    public Camara_Seguimiento camara;

    void Start()
    {
        // 1) Instanciar al jugador 1
        GameObject prefabJ1 = ObtenerPrefab(Datos_Partida.personajeJugador1);
        GameObject jugador1 = Instantiate(prefabJ1, spawnJugador1.position, spawnJugador1.rotation);

        // 2) Instanciar al jugador 2.
        //    NO le aplicamos flip manual de escala: el LookAtRival de los scripts
        //    PlayerMovement / PlayerController se encarga de orientar el visualPart
        //    cada frame según la posición del rival.
        GameObject prefabJ2 = ObtenerPrefab(Datos_Partida.personajeJugador2);
        GameObject jugador2 = Instantiate(prefabJ2, spawnJugador2.position, spawnJugador2.rotation);

        // 3) Asignar los rivales cruzados (cada jugador apunta al otro).
        AsignarRival(jugador1, jugador2.transform);
        AsignarRival(jugador2, jugador1.transform);

        // 4) Forzar el número de jugador en runtime.
        //    Esto sobreescribe el playerNumber del prefab para que el primer
        //    spawn use siempre los controles del Jugador 1 (WASD + O) y el
        //    segundo los del Jugador 2 (TFGH + P), sin importar qué personaje
        //    haya elegido cada uno.
        AsignarNumeroJugador(jugador1, 1);
        AsignarNumeroJugador(jugador2, 2);

        // 5) Asignar los Transforms a la cámara para que los siga
        if (camara != null)
        {
            camara.jugador1 = jugador1.transform;
            camara.jugador2 = jugador2.transform;
        }

        // 6) Cargar la arena seleccionada de forma aditiva.
        //    Antes, nos suscribimos al evento sceneLoaded para limpiar las cámaras
        //    y AudioListeners de la escena cargada.
        SceneManager.sceneLoaded += AlCargarArena;
        string nombreEscena = ObtenerNombreEscenario(Datos_Partida.escenario);
        SceneManager.LoadScene(nombreEscena, LoadSceneMode.Additive);
    }

    void AlCargarArena(Scene escenaCargada, LoadSceneMode modo)
    {
        // Solo nos interesan las cargas aditivas (la arena)
        if (modo != LoadSceneMode.Additive) return;

        // Desactivar todas las cámaras y AudioListeners de la escena de arena
        // para que la única cámara activa sea la de la escena Pelea.
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

        // Ya no necesitamos seguir escuchando este evento
        SceneManager.sceneLoaded -= AlCargarArena;
    }

    // Busca PlayerMovement y PlayerController en el jugador instanciado y les
    // asigna el Transform del rival. Usamos GetComponentInChildren (incluyendo
    // inactivos) por si el script vive en un hijo del prefab.
    void AsignarRival(GameObject jugador, Transform rival)
    {
        PlayerMovement movimiento = jugador.GetComponentInChildren<PlayerMovement>(true);
        if (movimiento != null) movimiento.rival = rival;

        PlayerController controlador = jugador.GetComponentInChildren<PlayerController>(true);
        if (controlador != null) controlador.rival = rival;
    }

    // Setea playerNumber tanto en PlayerMovement como en PlayerCombat para que
    // el jugador instanciado reciba el set de controles correcto en runtime,
    // independiente de lo que tenga el prefab.
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