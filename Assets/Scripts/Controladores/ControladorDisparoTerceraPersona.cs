using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.Animations.Rigging;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ControladorDisparoTerceraPersona : MonoBehaviour
{
    [Header("Referencias de Sistema")]
    [SerializeField] private SistemaCambioCamara sistemaCamara; 

    [SerializeField] private Rig rigApuntado;
    [SerializeField] private GameObject camaraVirtualApuntado;
    [SerializeField] private float sensibilidadNormal = 2f;
    [SerializeField] private float sensibilidadApuntado = 0.5f;
    [SerializeField] private LayerMask mascaraCapasApuntado;
    [SerializeField] private Transform transformDebug;
    [SerializeField] private Transform prefabProyectilBala;
    [SerializeField] private Transform posicionGeneracionBala;

    private ThirdPersonController controladorTerceraPersona;
    private StarterAssetsInputs entradasStarterAssets;
    private Animator animador;
    private float pesoRigApuntado;
    private Camera _camaraPrincipal; // Cache para evitar Camera.main (muy lento)

    private void Awake()
    {
        entradasStarterAssets = GetComponent<StarterAssetsInputs>();
        controladorTerceraPersona = GetComponent<ThirdPersonController>();
        animador = GetComponent<Animator>();
        _camaraPrincipal = Camera.main;
    }

    private void Start()
    {
        if (GestorPools.Instancia != null && prefabProyectilBala != null)
        {
            // Aumentamos el pool a 50 para evitar instanciar (lag) si disparas muy rápido
            GestorPools.Instancia.RegistrarGrupo("ProyectilBala", prefabProyectilBala.gameObject, 50);
        }
    }

    private void Update()
    {
        // Seguridad por si la cámara cambia o se pierde
        if (_camaraPrincipal == null) _camaraPrincipal = Camera.main;

        bool es3P = (sistemaCamara != null) ? sistemaCamara.EsTerceraPersona : true;

        Vector3 posicionRatonMundo = Vector3.zero;
        Vector2 puntoCentroPantalla = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray rayo = _camaraPrincipal.ScreenPointToRay(puntoCentroPantalla);

        Vector3 origenRayo = es3P ? rayo.origin : rayo.GetPoint(0.5f); 

        if (Physics.Raycast(origenRayo, rayo.direction, out RaycastHit impacto, 999f, mascaraCapasApuntado))
        {
            posicionRatonMundo = impacto.point;
        }
        else
        {
            posicionRatonMundo = rayo.GetPoint(999f);
        }

        if (transformDebug != null) 
        {
            transformDebug.position = Vector3.Lerp(transformDebug.position, posicionRatonMundo, Time.deltaTime * 40f);
        }


        // Capturamos el input de disparo (Fallback directo al ratón para asegurar que funcione)
        bool disparoInput = entradasStarterAssets.shoot;
#if ENABLE_INPUT_SYSTEM
        if (!disparoInput && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            disparoInput = true;
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        if (!disparoInput && Input.GetMouseButtonDown(0)) disparoInput = true;
#endif

        bool enModoCombate = !es3P || entradasStarterAssets.aim || disparoInput;

        if (enModoCombate)
        {
            if (!es3P || entradasStarterAssets.aim)
            {
                if (es3P) camaraVirtualApuntado.SetActive(true);
                controladorTerceraPersona.SetSensitivity(sensibilidadApuntado);
                animador.SetBool("IsAiming", true);
            }
            else
            {
                camaraVirtualApuntado.SetActive(false);
                controladorTerceraPersona.SetSensitivity(sensibilidadNormal);
                animador.SetBool("IsAiming", false); 
            }

            Vector3 frenteCamaraMundo = _camaraPrincipal.transform.forward;
            frenteCamaraMundo.y = 0; 

            if (frenteCamaraMundo != Vector3.zero && es3P)
            {
                controladorTerceraPersona.SetRotateOnMove(false);
                Quaternion rotacionObjetivo = Quaternion.LookRotation(frenteCamaraMundo);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 20f);
            }
            
            pesoRigApuntado = 1f;
        }
        else
        {
            camaraVirtualApuntado.SetActive(false);
            controladorTerceraPersona.SetSensitivity(sensibilidadNormal);
            controladorTerceraPersona.SetRotateOnMove(true);
            animador.SetBool("IsAiming", false);
            pesoRigApuntado = 0f;
        }

        rigApuntado.weight = Mathf.Lerp(rigApuntado.weight, pesoRigApuntado, Time.deltaTime * 20f);

        // Sonido de recarga manual
        bool recargaInput = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) recargaInput = true;
#elif ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.R)) recargaInput = true;
#endif

        if (recargaInput && GestorAudio.Instancia != null)
        {
            GestorAudio.Instancia.ReproducirRecarga();
        }

        if (disparoInput)
        {
            Debug.Log("ControladorDisparo: ¡PUM! Disparo detectado."); // Mensaje de control
            animador.SetTrigger("Shoot");

            Vector3 direccionApuntado = (posicionRatonMundo - posicionGeneracionBala.position).normalized;
            
            if (GestorPools.Instancia != null)
            {
                GestorPools.Instancia.GenerarDesdeGrupo("ProyectilBala", posicionGeneracionBala.position, Quaternion.LookRotation(direccionApuntado, Vector3.up));
            }
            else
            {
                Instantiate(prefabProyectilBala, posicionGeneracionBala.position, Quaternion.LookRotation(direccionApuntado, Vector3.up));
            }
            
            if (GestorAudio.Instancia != null)
            {
                GestorAudio.Instancia.ReproducirDisparo(posicionGeneracionBala.position);
            }
            else
            {
                Debug.LogWarning("ControladorDisparo: No se encuentra el objeto 'GestorAudio' en la escena.");
            }
            
            entradasStarterAssets.shoot = false;
        }
    }
}