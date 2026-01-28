using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioZombi : MonoBehaviour
{
    public AudioClip clipAmbienteZombi;
    private AudioSource fuenteAudio;

    void Awake()
    {
        fuenteAudio = GetComponent<AudioSource>();
        fuenteAudio.clip = clipAmbienteZombi;
        fuenteAudio.loop = true;
        fuenteAudio.spatialBlend = 1.0f;
        fuenteAudio.minDistance = 5f;
        fuenteAudio.maxDistance = 30f;
    }

    void Start()
    {
        fuenteAudio.Play();
    }
}
