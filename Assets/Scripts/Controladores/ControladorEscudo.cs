using UnityEngine;
using TMPro;
using System.Collections;

public class ControladorEscudo : MonoBehaviour
{
    [Header("Configuración del Escudo")]
    [Tooltip("Tiempo total que dura el escudo")]
    public float duracionEscudo = 10f; 
    
    [Header("UI")]
    public TMP_Text textoTiempo; 
    [Tooltip("Tiempo restante para empezar a parpadear el texto")]
    public float tiempoInicioParpadeo = 3.0f;

    private string tagOriginal;
    private Coroutine corrutinaEscudo;

    private void Start()
    {
        // Guardamos el tag (ej: "Player") para devolverlo luego
        tagOriginal = gameObject.tag; 
        
        if (textoTiempo != null) 
            textoTiempo.gameObject.SetActive(false);
    }

    public void ActivarEscudoDesdeItem()
    {
        // Si ya hay un escudo activo, lo reiniciamos
        if (corrutinaEscudo != null) StopCoroutine(corrutinaEscudo);
        
        if (GestorAudio.Instancia != null) GestorAudio.Instancia.ReproducirPickupEscudo();
        corrutinaEscudo = StartCoroutine(RutinaEscudo(duracionEscudo));
    }

    private IEnumerator RutinaEscudo(float tiempo)
    {
        // CAMBIO DE ESTADO: Los zombies dejan de seguirte al ser "Untagged"
        gameObject.tag = "Untagged"; 
        
        if (textoTiempo != null) 
        {
            textoTiempo.gameObject.SetActive(true);
            textoTiempo.enabled = true; 
        }

        float tiempoRestante = tiempo;

        while (tiempoRestante > 0)
        {
            if (textoTiempo != null)
            {
                textoTiempo.SetText("Escudo: {0:0.0}s", tiempoRestante);

                // Lógica de parpadeo del texto UI cuando queda poco tiempo
                if (tiempoRestante <= tiempoInicioParpadeo)
                {
                    float fase = (tiempoRestante * 5f) % 1f; 
                    textoTiempo.enabled = fase > 0.2f; 
                }
            }
            
            yield return null;
            tiempoRestante -= Time.deltaTime;
        }

        FinalizarEscudo();
    }

    private void FinalizarEscudo()
    {
        if (textoTiempo != null)
        {
            textoTiempo.enabled = true; 
            textoTiempo.gameObject.SetActive(false);
        }
        
        // VOLVEMOS AL ESTADO NORMAL: Los zombies volverán a por ti
        gameObject.tag = tagOriginal; 
        corrutinaEscudo = null;
    }
}