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

    public static int victoriasJ1 = 0;
    public static int victoriasJ2 = 0;
    public static int rondaActual = 1;

    public static void ReiniciarRondas()
    {
        victoriasJ1 = 0;
        victoriasJ2 = 0;
        rondaActual = 1;
    }
}