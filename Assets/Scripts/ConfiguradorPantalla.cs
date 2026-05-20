using UnityEngine;

public class ConfiguradorPantalla : MonoBehaviour
{
    void Awake()
    {
        Screen.SetResolution(1280, 960, FullScreenMode.ExclusiveFullScreen);

        Application.runInBackground = true;
    }
}