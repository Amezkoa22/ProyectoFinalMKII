using UnityEngine;
using UnityEngine.SceneManagement; 
public class VolverAlMenu : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Escribe aquí el nombre exacto de la escena de tu menú principal")]
    public string escenaMenuPrincipal;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!string.IsNullOrEmpty(escenaMenuPrincipal))
            {
                SceneManager.LoadScene(escenaMenuPrincipal);
            }
            else
            {
                Debug.LogWarning("¡No has puesto el nombre de la escena del Menú en el Inspector!");
            }
        }
    }
}