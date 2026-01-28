using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GestorJuego : MonoBehaviour
{
    // Singleton
    public static GestorJuego Instancia { get; private set; }

    public event Action<int> AlCambiarPuntuacion;
    public event Action<int, int> AlCambiarZombis;
    public event Action<int> AlCambiarVidas;
    public event Action<bool> AlCambiarEstadoPausa;

    [Header("Nombres de Escenas")]
    public string nombreEscenaJuego = "Juego";
    public string nombreEscenaMenu = "MenuPrincipal";
    public string nombreEscenaVictoria = "Victoria";
    public string nombreEscenaDerrota = "Derrota";

    [Header("Estado del Juego")]
    public int Vidas = 3;
    public int Puntuacion { get; private set; }
    public int ZombisMatados { get; private set; }
    public int TotalZombis { get; private set; }
    public bool EstaPausado { get; private set; }
    private bool juegoTerminado = false;
    
    [Header("Configuración de Daño")]
    public float tiempoInvulnerabilidad = 2.0f;
    private bool esInvulnerable = false;
    
    public void Reanudar()
    {
        AlternarPausa();
    }

    public void Jugar()
    {
        ReiniciarEstadisticas();
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void IrAlMenu()
    {
        ReiniciarEstadisticas();
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    [ContextMenu("Salir del Juego")]
    public void Salir()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            AlternarPausa();
        }
    }

    public void AlternarPausa()
    {
        EstaPausado = !EstaPausado;
        Time.timeScale = EstaPausado ? 0f : 1f;

        Cursor.lockState = EstaPausado ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = EstaPausado;
        
        if (juegoTerminado)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = EstaPausado ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = EstaPausado;
        }
        
        AlCambiarEstadoPausa?.Invoke(EstaPausado);
    }

    public void ReiniciarEstadisticas()
    {
        Puntuacion = 0;
        ZombisMatados = 0;
        Vidas = 3;
        EstaPausado = false;
        juegoTerminado = false;
        esInvulnerable = false;
        Time.timeScale = 1f;
        if (GestorAudio.Instancia != null) GestorAudio.Instancia.ActualizarLatido(false);
    }

    public void SumarPuntuacion(int cantidad) { Puntuacion += cantidad; AlCambiarPuntuacion?.Invoke(Puntuacion); }
    public void RegistrarNuevoZombie() { TotalZombis++; AlCambiarZombis?.Invoke(ZombisMatados, TotalZombis); }
    
    public void DefinirTotalZombis(int total) 
    { 
        TotalZombis = total; 
        AlCambiarZombis?.Invoke(ZombisMatados, TotalZombis); 
    }

    public void SumarMuerteZombi() 
    { 
        ZombisMatados++; 
        AlCambiarZombis?.Invoke(ZombisMatados, TotalZombis); 
        
        if (ZombisMatados >= TotalZombis && TotalZombis > 0)
        {
            GanarJuego();
        }
    }
    
    public void PerderVida()
    {
        if (juegoTerminado || esInvulnerable) return; 

        StartCoroutine(RutinaInvulnerabilidad());

        if (Vidas > 0) 
        { 
            Vidas--; 
            AlCambiarVidas?.Invoke(Vidas);
            if (GestorAudio.Instancia != null) GestorAudio.Instancia.ReproducirPerderVida();
        }
        
        // Activar latido si queda 1 vida
        if (Vidas == 1 && GestorAudio.Instancia != null)
        {
            GestorAudio.Instancia.ActualizarLatido(true);
        }

        if (Vidas <= 0) 
        {
            if (GestorAudio.Instancia != null) GestorAudio.Instancia.ActualizarLatido(false);
            juegoTerminado = true;
            StartCoroutine(EsperarYTerminar(nombreEscenaDerrota));
        }
    }

    private IEnumerator RutinaInvulnerabilidad()
    {
        esInvulnerable = true;
        Debug.Log("¡Jugador herido! Invulnerable por " + tiempoInvulnerabilidad + " segundos.");
        
        yield return new WaitForSeconds(tiempoInvulnerabilidad);
        
        esInvulnerable = false;
        Debug.Log("Jugador vulnerable de nuevo.");
    }

    public void GanarJuego()
    {
        juegoTerminado = true;
        TerminarPartida(nombreEscenaVictoria);
    }

    private IEnumerator EsperarYTerminar(string escena)
    {
        yield return new WaitForSeconds(0.5f);
        TerminarPartida(escena);
    }

    private void TerminarPartida(string nombreEscena)
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(nombreEscena);
    }

    public void ActualizarVidas(int vidas)
    {
        Vidas = vidas;
        AlCambiarVidas?.Invoke(Vidas);
    }
}