// Clase estática y enums que comparten el menú de selección y la escena de pelea.
// El menú de selección escribe los valores antes de cargar Pelea;
// Cargador_Pelea los lee en su Start() para instanciar todo correctamente.

public enum TipoPersonaje
{
    Scorpion,
    SubZero,
    Kitana
}

public enum TipoEscenario
{
    DeadPool,
    KansArena,
    Armory
}

public static class Datos_Partida
{
    public static TipoPersonaje personajeJugador1;
    public static TipoPersonaje personajeJugador2;
    public static TipoEscenario escenario;
}
