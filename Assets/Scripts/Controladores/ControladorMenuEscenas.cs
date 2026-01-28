using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorMenuEscenas : MonoBehaviour
{
    [Header("Nombres de las Escenas")]
    [Tooltip("Escribe aquí el nombre EXACTO de tu escena de juego (ej: Juego, Nivel1)")]
    public string nombreEscenaJuego = "Juego";
    
    [Tooltip("Escribe aquí el nombre EXACTO de tu escena de menú (ej: MenuPrincipal)")]
    public string nombreEscenaMenu = "MenuPrincipal";

    [Tooltip("Escribe aquí el nombre EXACTO de tu escena de victoria")]
    public string nombreEscenaVictoria = "Victoria";

    [Tooltip("Escribe aquí el nombre EXACTO de tu escena de derrota")]
    public string nombreEscenaDerrota = "Derrota";

    [Header("Referencias UI")]
    [Tooltip("Arrastra aquí el Canvas de Pausa de esta escena")]
    public GameObject canvasMenuPausa;

    void Start()
    {
        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.AlCambiarEstadoPausa += ActualizarMenuPausa;
            
            if (canvasMenuPausa != null)
            {
                canvasMenuPausa.SetActive(GestorJuego.Instancia.EstaPausado);
            }
        }
    }

    void OnDestroy()
    {
        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.AlCambiarEstadoPausa -= ActualizarMenuPausa;
        }
    }

    public void Reanudar()
    {
        if (GestorJuego.Instancia != null) GestorJuego.Instancia.Reanudar();
    }

    public void VolverAJugar()
    {
        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.Jugar();
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nombreEscenaJuego);
        }
    }

    public void IrAlMenu()
    {
        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.IrAlMenu();
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nombreEscenaMenu);
        }
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void ActualizarMenuPausa(bool estaPausado)
    {
        if (canvasMenuPausa != null) canvasMenuPausa.SetActive(estaPausado);
    }
}
