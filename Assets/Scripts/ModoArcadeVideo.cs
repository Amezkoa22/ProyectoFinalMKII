using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;

public class ModoArcadeVideo : MonoBehaviour
{
    [Header("Componentes de UI")]
    public VideoPlayer videoPlayer;
    public CanvasGroup canvasGroupVideo;
    public RawImage pantallaRawImage;

    [Header("Ajustes de Tiempo")]
    public float tiempoEsperaInactivo = 7f;
    public float velocidadFade = 1.5f;

    [Header("Lista de Videos (Clips)")]
    public List<VideoClip> videosDemostracion;

    private float cronometroInactividad;
    private bool videoReproduciendo = false;
    private bool haciendoFade = false;
    private int ultimoVideoIndex = -1;
    private Coroutine corrutinaActual;

    void Start()
    {
        if (canvasGroupVideo != null)
        {
            canvasGroupVideo.alpha = 0f;
            canvasGroupVideo.blocksRaycasts = false;
        }

        if (videoPlayer != null)
        {
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
            videoPlayer.loopPointReached += AlTerminarVideo;
        }
    }

    void Update()
    {
        if (videoReproduciendo && !haciendoFade)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                DetenerCorrutinaSegura();
                corrutinaActual = StartCoroutine(TerminarYQuitarVideo());
            }
        }

        if (!videoReproduciendo && !haciendoFade)
        {
            if (Input.anyKey || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            {
                cronometroInactividad = 0f;
            }
            else
            {
                cronometroInactividad += Time.deltaTime;
            }

            if (cronometroInactividad >= tiempoEsperaInactivo)
            {
                DetenerCorrutinaSegura();
                corrutinaActual = StartCoroutine(IniciarVideoAleatorio());
            }
        }
    }

    System.Collections.IEnumerator IniciarVideoAleatorio()
    {
        if (videosDemostracion == null || videosDemostracion.Count == 0 || videoPlayer == null || pantallaRawImage == null) yield break;
        haciendoFade = true;

        int indexAleatorio = Random.Range(0, videosDemostracion.Count);
        if (videosDemostracion.Count > 1 && indexAleatorio == ultimoVideoIndex)
        {
            indexAleatorio = (indexAleatorio + 1) % videosDemostracion.Count;
        }
        ultimoVideoIndex = indexAleatorio;

        videoPlayer.clip = videosDemostracion[indexAleatorio];
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        float tiempoLimite = 0f;
        while (videoPlayer.texture == null)
        {
            tiempoLimite += Time.deltaTime;
            if (tiempoLimite >= 2f)
            {
                videoPlayer.Stop();
                haciendoFade = false;
                cronometroInactividad = 0f;
                yield break;
            }
            yield return null;
        }

        pantallaRawImage.texture = videoPlayer.texture;
        videoPlayer.Play();
        videoReproduciendo = true;

        while (canvasGroupVideo != null && canvasGroupVideo.alpha < 1f)
        {
            canvasGroupVideo.alpha += Time.deltaTime * velocidadFade;
            yield return null;
        }

        if (canvasGroupVideo != null) canvasGroupVideo.blocksRaycasts = true;
        haciendoFade = false;
    }

    System.Collections.IEnumerator TerminarYQuitarVideo()
    {
        haciendoFade = true;
        if (canvasGroupVideo != null) canvasGroupVideo.blocksRaycasts = false;

        while (canvasGroupVideo != null && canvasGroupVideo.alpha > 0f)
        {
            canvasGroupVideo.alpha -= Time.deltaTime * velocidadFade;
            yield return null;
        }

        if (videoPlayer != null) videoPlayer.Stop();
        if (pantallaRawImage != null) pantallaRawImage.texture = null;

        videoReproduciendo = false;
        haciendoFade = false;
        cronometroInactividad = 0f;
    }

    void AlTerminarVideo(VideoPlayer vp)
    {
        if (this != null && gameObject.activeInHierarchy && videoReproduciendo)
        {
            DetenerCorrutinaSegura();
            corrutinaActual = StartCoroutine(TerminarYQuitarVideo());
        }
    }

    void DetenerCorrutinaSegura()
    {
        if (corrutinaActual != null)
        {
            StopCoroutine(corrutinaActual);
            corrutinaActual = null;
        }
    }
}