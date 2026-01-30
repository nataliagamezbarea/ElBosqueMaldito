using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class GestorAudio : MonoBehaviour
{
    public static GestorAudio Instancia;

    private AudioSource fuenteMusica;
    private AudioSource fuenteEfectos; 
    private AudioSource fuenteTension; 
    private AudioSource fuenteLatido;
    private List<AudioSource> fuentes3DPool;
    private int proximaFuente3D = 0;
    private const int TAMANO_POOL_3D = 15;

    [Header("Música de Fondo")]
    public AudioClip musicaJuego;
    public AudioClip musicaVictoria;
    public AudioClip musicaDerrota;
    public AudioClip sonidoTension;

    [Header("Acciones Jugador")]
    public AudioClip sonidoDisparo;
    public AudioClip sonidoRecarga;
    public AudioClip sonidoGrito;
    public AudioClip sonidoPickupVida;
    public AudioClip sonidoLatido;

    [Header("Escudo")]
    public AudioClip sonidoPickupEscudo;

    [Header("Zombis Global")]
    public AudioClip sonidoMordida;
    public AudioClip sonidoMuerteZombi;

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);

            // --- Configuración Automática de Fuentes ---
            fuenteMusica = gameObject.AddComponent<AudioSource>();
            fuenteMusica.loop = true;

            fuenteEfectos = gameObject.AddComponent<AudioSource>();

            fuenteTension = gameObject.AddComponent<AudioSource>();
            fuenteTension.loop = true;
            fuenteTension.volume = 0f;

            fuenteLatido = gameObject.AddComponent<AudioSource>();
            fuenteLatido.loop = true;

            // --- Pool para Efectos 3D ---
            fuentes3DPool = new List<AudioSource>(TAMANO_POOL_3D);
            for (int i = 0; i < TAMANO_POOL_3D; i++)
            {
                AudioSource nuevaFuente = gameObject.AddComponent<AudioSource>();
                nuevaFuente.spatialBlend = 1.0f; // Sonido 3D
                nuevaFuente.playOnAwake = false;
                fuentes3DPool.Add(nuevaFuente);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += AlCargarEscena;

        if (fuenteTension != null && sonidoTension != null)
        {
            fuenteTension.clip = sonidoTension;
            if (!fuenteTension.isPlaying) fuenteTension.Play();
        }
        else if (sonidoTension == null)
        {
            Debug.LogWarning("GestorAudio: El campo 'Sonido Tension' está vacío.");
        }

        if (fuenteLatido != null && sonidoLatido != null)
        {
            fuenteLatido.clip = sonidoLatido;
            if (fuenteLatido.isPlaying) fuenteLatido.Stop();
        }
    }

    private void Start()
    {
        ActualizarMusica(SceneManager.GetActiveScene().name);
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        ActualizarMusica(escena.name);
        if (fuenteTension != null) fuenteTension.volume = 0f;
        if (fuenteLatido != null) fuenteLatido.Stop();
        if (fuenteEfectos != null) fuenteEfectos.Stop();
    }

    public void ActualizarMusica(string nombreEscena)
    {
        AudioClip clipAReproducir = musicaJuego;

        if (nombreEscena.IndexOf("victoria", StringComparison.OrdinalIgnoreCase) >= 0) clipAReproducir = musicaVictoria;
        else if (nombreEscena.IndexOf("derrota", StringComparison.OrdinalIgnoreCase) >= 0) clipAReproducir = musicaDerrota;

        if (fuenteMusica != null)
        {
            if (clipAReproducir != null)
            {
                if (fuenteMusica.clip != clipAReproducir)
                {
                    fuenteMusica.clip = clipAReproducir;
                    fuenteMusica.loop = true;
                    fuenteMusica.Play();
                }
            }
            else
            {
                fuenteMusica.Stop();
                fuenteMusica.clip = null;
            }
        }
    }

    private void ReproducirEnPunto(AudioClip clip, Vector3 posicion, float volumen = 1f)
    {
        if (clip == null) return;
        
        // Coge la siguiente fuente del pool en modo round-robin
        AudioSource fuente = fuentes3DPool[proximaFuente3D];
        proximaFuente3D = (proximaFuente3D + 1) % TAMANO_POOL_3D;

        fuente.transform.position = posicion;
        fuente.PlayOneShot(clip, volumen);
    }

    public void ReproducirDisparo(Vector3 position)
    {
        if (sonidoDisparo != null) ReproducirEnPunto(sonidoDisparo, position);
        else Debug.LogWarning("GestorAudio: El sonido 'Sonido Disparo' no está asignado.");
    }

    public void ReproducirSonidoMuerteZombi(Vector3 position)
    {
        if (sonidoMuerteZombi != null) ReproducirEnPunto(sonidoMuerteZombi, position, 0.6f);
        else Debug.LogWarning("GestorAudio: El sonido 'Sonido Muerte Zombi' no está asignado.");
    }
    
    public void ReproducirRecarga() => PlaySfx(sonidoRecarga, "Sonido Recarga");
    public void ReproducirMordida() => PlaySfx(sonidoMordida, "Sonido Mordida");
    public void ReproducirPickupEscudo() => PlaySfx(sonidoPickupEscudo, "Sonido Pickup Escudo");
    public void ReproducirPickupVida() => PlaySfx(sonidoPickupVida, "Sonido Pickup Vida");
    public void ReproducirMuerteJugador() => PlaySfx(sonidoGrito, "Sonido Grito", 0.6f);
    public void ReproducirPerderVida() => PlaySfx(sonidoGrito, "Sonido Grito", 0.6f);
    public void ReproducirGrito() => PlaySfx(sonidoGrito, "Sonido Grito", 0.6f);

    private void PlaySfx(AudioClip clip, string nombreParametro = "AudioClip", float volumen = 1f)
    {
        if (clip != null) 
        {
            fuenteEfectos.PlayOneShot(clip, volumen);
        }
        else
        {
            Debug.LogWarning($"GestorAudio: El sonido '{nombreParametro}' no está asignado en el Inspector.");
        }
    }

    public void ActualizarTension(float intensidad)
    {
        if (fuenteTension != null)
        {
            float velocidadCambio = intensidad < 0.01f ? 0.5f : 2f;
            fuenteTension.volume = Mathf.Lerp(fuenteTension.volume, intensidad, Time.deltaTime * 2f);
        }
    }

    public void ActualizarLatido(bool activar)
    {
        if (fuenteLatido != null)
        {
            if (activar && !fuenteLatido.isPlaying) fuenteLatido.Play();
            else if (!activar && fuenteLatido.isPlaying) fuenteLatido.Stop();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }
}