using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfiguracionUI : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Arrastra aquí el objeto de texto para la puntuación (opcional)")]
    public TMP_Text textoPuntuacion;

    [Tooltip("Arrastra aquí el objeto de texto para los zombis (opcional)")]
    public TMP_Text textoZombisMatados;

    [Tooltip("Arrastra aquí las imágenes de los corazones (opcional)")]
    public Image[] corazones;

    [Header("UI Sprites")]
    [Tooltip("Arrastra aquí el sprite de corazón lleno")]
    public Sprite corazonLleno;
    [Tooltip("Arrastra aquí el sprite de corazón vacío")]
    public Sprite corazonVacio;

    void OnEnable()
    {
        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.AlCambiarPuntuacion += ActualizarTextoPuntuacion;
            GestorJuego.Instancia.AlCambiarZombis += ActualizarTextoZombis;
            GestorJuego.Instancia.AlCambiarVidas += ActualizarVidasUI;
        }
    }

    void OnDisable()
    {
        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.AlCambiarPuntuacion -= ActualizarTextoPuntuacion;
            GestorJuego.Instancia.AlCambiarZombis -= ActualizarTextoZombis;
            GestorJuego.Instancia.AlCambiarVidas -= ActualizarVidasUI;
        }
    }

    void Start()
    {
        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.AlCambiarPuntuacion -= ActualizarTextoPuntuacion;
            GestorJuego.Instancia.AlCambiarPuntuacion += ActualizarTextoPuntuacion;
            GestorJuego.Instancia.AlCambiarZombis -= ActualizarTextoZombis;
            GestorJuego.Instancia.AlCambiarZombis += ActualizarTextoZombis;
            GestorJuego.Instancia.AlCambiarVidas -= ActualizarVidasUI;
            GestorJuego.Instancia.AlCambiarVidas += ActualizarVidasUI;

            ActualizarTextoPuntuacion(GestorJuego.Instancia.Puntuacion);
            ActualizarTextoZombis(GestorJuego.Instancia.ZombisMatados, GestorJuego.Instancia.TotalZombis);
            ActualizarVidasUI(GestorJuego.Instancia.Vidas);
        }
        else
        {
            Debug.LogError("UISetup no pudo encontrar una instancia de GameManager.");
        }
    }

    void ActualizarTextoPuntuacion(int puntuacion)
    {
        if (textoPuntuacion != null)
        {
            textoPuntuacion.text = "Puntuación: " + puntuacion;
        }
    }

    void ActualizarTextoZombis(int matados, int total)
    {
        if (textoZombisMatados != null)
        {
            textoZombisMatados.text = "Zombies: " + matados + "/" + total;
        }
    }

    void ActualizarVidasUI(int vidas)
    {
        if (corazones == null || corazones.Length == 0 || corazonLleno == null || corazonVacio == null) return;

        for (int i = 0; i < corazones.Length; i++)
        {
            if (corazones[i] != null)
            {
                corazones[i].sprite = (i < vidas) ? corazonLleno : corazonVacio;
            }
        }
    }
}
