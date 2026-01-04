using UnityEngine;
using System;

[System.Serializable]
public class Sound
{
    public string nombre;
    public AudioClip clip;
    [Range(0f, 1f)] public float volumen = 0.5f;
    [Range(0.1f, 3f)] public float tono = 1f;
    public bool loop = false;

    [HideInInspector] public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Lista de Sonidos")]
    public Sound[] sonidos;

    void Awake()
    {
        // Configuración del Singleton
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        
        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sonidos)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volumen;
            s.source.pitch = s.tono;
            s.source.loop = s.loop;
        }
    }

    public void Play(string nombre)
    {
        Sound s = Array.Find(sonidos, sound => sound.nombre == nombre);
        if (s == null)
        {
            Debug.LogWarning("Sonido: " + nombre + " no encontrado!");
            return;
        }
        s.source.Play();
    }

    // Útil para sonidos que se repiten rápido como disparos
    public void PlayOneShot(string nombre)
    {
        Sound s = Array.Find(sonidos, sound => sound.nombre == nombre);
        if (s != null)
        {
            s.source.PlayOneShot(s.clip);
        }
    }
}