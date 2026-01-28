using UnityEngine;
using UnityEngine.SceneManagement;

public class GestorAudio : MonoBehaviour
{
    public static GestorAudio Instancia;

    // Fuentes internas (se crean solas, no hace falta arrastrarlas)
    private AudioSource fuenteMusica;
    private AudioSource fuenteEfectos; 
    private AudioSource fuenteTension; 
    private AudioSource fuenteLatido;

    [Header("Música de Fondo")]
    public AudioClip musicaJuego;
    public AudioClip musicaVictoria;
    public AudioClip musicaDerrota;
    public AudioClip sonidoTension; // Nuevo campo para el loop de tensión

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
            
            // 1. Música (Loop, volumen normal)
            fuenteMusica = gameObject.AddComponent<AudioSource>();
            fuenteMusica.loop = true;

            // 2. Efectos (Sin loop)
            fuenteEfectos = gameObject.AddComponent<AudioSource>();

            // 3. Tensión (Loop, empieza en silencio)
            fuenteTension = gameObject.AddComponent<AudioSource>();
            fuenteTension.loop = true;
            fuenteTension.volume = 0f;

            // 4. Latido (Loop, empieza parado)
            fuenteLatido = gameObject.AddComponent<AudioSource>();
            fuenteLatido.loop = true;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += AlCargarEscena;

        // Asignar clips de loops
        if (fuenteTension != null && sonidoTension != null)
        {
            fuenteTension.clip = sonidoTension;
            if (!fuenteTension.isPlaying) fuenteTension.Play();
        }
        else if (sonidoTension == null)
        {
            Debug.LogWarning("GestorAudio: El campo 'Sonido Tension' está vacío. No habrá música de tensión.");
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
    }

    public void ActualizarMusica(string nombreEscena)
    {
        AudioClip clipAReproducir = musicaJuego;

        string nombreLower = nombreEscena.ToLower();
        if (nombreLower.Contains("victoria")) clipAReproducir = musicaVictoria;
        else if (nombreLower.Contains("derrota")) clipAReproducir = musicaDerrota;

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
                // Si no hay música asignada (ej. Menu vacío), silencio total
                fuenteMusica.Stop();
                fuenteMusica.clip = null;
            }
        }
    }

    public void ReproducirDisparo(Vector3 position)
    {
        if (sonidoDisparo != null) AudioSource.PlayClipAtPoint(sonidoDisparo, position);
        else Debug.LogWarning("GestorAudio: El sonido 'Sonido Disparo' no está asignado.");
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

    public void ReproducirSonidoMuerteZombi(Vector3 position)
    {
        if (sonidoMuerteZombi != null) 
        {
            AudioSource.PlayClipAtPoint(sonidoMuerteZombi, position);
        }
        else
        {
            Debug.LogWarning("GestorAudio: El sonido 'Sonido Muerte Zombi' no está asignado.");
        }
    }

    public void ActualizarTension(float intensidad)
    {
        if (fuenteTension != null)
        {
            // Si la intensidad es muy baja, bajamos el volumen rápido para que se calle pronto
            float velocidadCambio = intensidad < 0.01f ? 0.5f : 2f;
            fuenteTension.volume = Mathf.Lerp(fuenteTension.volume, intensidad, Time.deltaTime * 2f);
            
            // Debug opcional: Descomenta esto si quieres ver en consola cuándo sube la tensión
            // if (intensidad > 0.1f) Debug.Log($"Tensión actual: {intensidad}");
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