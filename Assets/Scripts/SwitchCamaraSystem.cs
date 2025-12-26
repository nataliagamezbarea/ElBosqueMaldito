using UnityEngine;
using StarterAssets;

public class SwitchCameraSystem : MonoBehaviour
{
    [Header("Configuración de Inicio")]
    [Tooltip("¿Empezar en Tercera Persona? Si se desmarca, empieza en Primera.")]
    public bool empezarEnTerceraPersona = true;
    
    [Tooltip("Tecla para cambiar de cámara durante el juego.")]
    public KeyCode teclaCambio = KeyCode.C;

    [Header("Scripts")]
    public MonoBehaviour thirdPersonScript; 
    public MonoBehaviour firstPersonScript;

    [Header("Cámaras")]
    public GameObject camara3P;
    public GameObject camara1P;

    private bool esTerceraPersona;

    void Start()
    {
        esTerceraPersona = empezarEnTerceraPersona;
        ActualizarEstado();
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaCambio))
        {
            esTerceraPersona = !esTerceraPersona;
            ActualizarEstado();
        }
    }

    void ActualizarEstado()
    {
        if (thirdPersonScript != null) thirdPersonScript.enabled = esTerceraPersona;
        if (firstPersonScript != null) firstPersonScript.enabled = !esTerceraPersona;

        if (camara3P != null) camara3P.SetActive(esTerceraPersona);
        if (camara1P != null) camara1P.SetActive(!esTerceraPersona);
    }
}