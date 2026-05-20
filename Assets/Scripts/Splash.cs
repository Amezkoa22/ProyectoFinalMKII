using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class ControladorSplashVideo : MonoBehaviour
{
    [Header("Componentes de UI")]
    public VideoPlayer videoPlayer;
    public RawImage pantallaRawImage;

    [Header("Configuración de Escena")]
    [Tooltip("Escribe aquí el nombre exacto de la escena de tu menú principal")]
    public string escenaMenuPrincipal;

    private bool cambiandoDeEscena = false;

    void Start()
    {
        if (videoPlayer != null && pantallaRawImage != null)
        {
            videoPlayer.renderMode = VideoRenderMode.APIOnly;

            videoPlayer.loopPointReached += AlTerminarVideo;

            videoPlayer.Prepare();
            StartCoroutine(EsperarTexturaVideo());
        }
    }

    System.Collections.IEnumerator EsperarTexturaVideo()
    {
        while (!videoPlayer.isPrepared) yield return null;
        while (videoPlayer.texture == null) yield return null;

        pantallaRawImage.texture = videoPlayer.texture;
        videoPlayer.Play();
    }

    void Update()
    {
        if (!cambiandoDeEscena)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
            {
                IrAlMenu();
            }
        }
    }

    void AlTerminarVideo(VideoPlayer vp)
    {        IrAlMenu();
    }

    void IrAlMenu()
    {
        if (cambiandoDeEscena) return;
        cambiandoDeEscena = true;

        if (videoPlayer != null) videoPlayer.Stop();

        if (!string.IsNullOrEmpty(escenaMenuPrincipal))
        {
            SceneManager.LoadScene(escenaMenuPrincipal);
        }
        else
        {
            Debug.LogWarning("¡Olvidaste poner el nombre de la escena del menú en el Inspector del Splash!");
        }
    }
}