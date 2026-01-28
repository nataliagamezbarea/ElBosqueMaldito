using UnityEngine;
using StarterAssets;

public class SistemaCambioCamara : MonoBehaviour
{
    [Header("Configuración de Inicio")]
    public bool empezarEnTerceraPersona = true;
    public KeyCode teclaCambio = KeyCode.C;

    [Header("Scripts")]
    public MonoBehaviour scriptTerceraPersona; 
    public MonoBehaviour scriptPrimeraPersona;

    [Header("Cámaras")]
    public GameObject camara3P;
    public GameObject camara1P;

    private bool esTerceraPersona;

    public bool EsTerceraPersona => esTerceraPersona;

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
        if (scriptTerceraPersona != null) scriptTerceraPersona.enabled = esTerceraPersona;
        if (scriptPrimeraPersona != null) scriptPrimeraPersona.enabled = !esTerceraPersona;

        if (camara3P != null) camara3P.SetActive(esTerceraPersona);
        if (camara1P != null) camara1P.SetActive(!esTerceraPersona);
    }
}