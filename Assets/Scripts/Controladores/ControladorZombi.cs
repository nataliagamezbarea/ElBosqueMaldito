﻿using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ControladorZombi : MonoBehaviour
{
    private enum EstadoZombi { Persiguiendo, Atacando, Retirandose, Deambulando }
    private EstadoZombi estadoActual;

    private Animator animador;
    private NavMeshAgent agente;

    private static Transform jugador;
    private HabilidadesJugador habilidadesJugador;
    private SaludJugador saludJugador;
    private GeneradorZombis miGenerador;
    public string claveGrupo { get; private set; }

    [Header("Ajustes")]
    public float distanciaAtaque = 1.5f;
    public float tiempoEntreAtaques = 2f;
    public float duracionRetirada = 1.5f;
    private float tiempoUltimoAtaque;
    public int saludMaxima = 3;
    private int saludActual;
    private float tiempoSiguienteEstado = 0f;
    private static ControladorZombi atacanteActual = null;
    private bool estaMuerto = false;
    private float distanciaAtaqueSqr; // Cache de la distancia al cuadrado
    private float velocidadCorrer;
    private float velocidadCaminar;

    [Header("Audio Local")]
    public AudioClip[] sonidosPasos;
    public AudioClip[] sonidosAmbiente;
    public AudioClip[] sonidosCerca;
    private AudioSource audioSourcePropio;
    private float tiempoSiguienteGemido;
    private float tiempoSiguientePaso;

    void Awake()
    {
        animador = GetComponent<Animator>();
        agente = GetComponent<NavMeshAgent>();
        
        velocidadCorrer = Random.Range(2.5f, 4.5f);
        velocidadCaminar = velocidadCorrer * 0.5f; // Caminan a la mitad de velocidad
        agente.speed = velocidadCorrer;
        agente.angularSpeed = Random.Range(100f, 200f); 
        
        if (jugador == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                jugador = playerObj.transform;
                habilidadesJugador = jugador.GetComponent<HabilidadesJugador>();
                saludJugador = jugador.GetComponent<SaludJugador>();
            }
        }

        // Configuración de Audio 3D para que solo se oiga si está cerca
        audioSourcePropio = GetComponent<AudioSource>();
        if (audioSourcePropio == null) audioSourcePropio = gameObject.AddComponent<AudioSource>();
        
        audioSourcePropio.spatialBlend = 1.0f; // 1.0 = Totalmente 3D
        audioSourcePropio.minDistance = 2f;
        audioSourcePropio.maxDistance = 15f;   // Dejan de oírse a 15 metros
        audioSourcePropio.rolloffMode = AudioRolloffMode.Linear;
        tiempoSiguienteGemido = Time.time + Random.Range(2f, 10f);
    }

    void Start()
    {
        // Si el zombi está puesto manualmente en la escena, aseguramos que tenga vida
        if (saludActual <= 0) saludActual = saludMaxima;
        distanciaAtaqueSqr = distanciaAtaque * distanciaAtaque; // Pre-calculamos
    }

    void OnDisable()
    {
        if (atacanteActual == this) atacanteActual = null;
    }

    public void ReiniciarZombi(float nuevaVelocidad, GeneradorZombis referenciaGenerador, string clave)
    {
        saludActual = saludMaxima;
        claveGrupo = clave;
        miGenerador = referenciaGenerador;
        estaMuerto = false;
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
        if (animador != null) animador.SetBool("die", false);

        if (agente != null) 
        {
            velocidadCorrer = nuevaVelocidad;
            velocidadCaminar = nuevaVelocidad * 0.5f;
            agente.enabled = true;
            agente.isStopped = false;
            agente.speed = velocidadCorrer;
        }
        estadoActual = EstadoZombi.Persiguiendo;
    }

    void Update()
    {
        if (estaMuerto) return;
        if (GestorJuego.Instancia != null && GestorJuego.Instancia.EstaPausado) return;

        // Si el jugador no existe o el agente está mal
        if (jugador == null || agente == null || !agente.isOnNavMesh || !agente.enabled)
        {
            DetenerZombi();
            return;
        }

        // --- CORRECCIÓN CRÍTICA PARA EL ESCUDO ---
        if (jugador.CompareTag("Untagged"))
        {
            if (estadoActual != EstadoZombi.Deambulando)
            {
                if (atacanteActual == this) atacanteActual = null;
                estadoActual = EstadoZombi.Deambulando;
                BuscarDestinoAleatorio();
            }
            EstadoDeambulacion();
            return;
        }
        else if (estadoActual == EstadoZombi.Deambulando)
        {
            estadoActual = EstadoZombi.Persiguiendo;
        }

        switch (estadoActual)
        {
            case EstadoZombi.Persiguiendo:
                EstadoPersecucion();
                break;
            case EstadoZombi.Atacando:
                EstadoAtaque();
                break;
            case EstadoZombi.Retirandose:
                EstadoRetirada();
                break;
        }

        ProcesarSonidos();
    }

    // --- MÉTODOS DE CONTROL DE MOVIMIENTO ---

    private void ControlarMovimiento(float speed, bool run, bool walk)
    {
        agente.isStopped = false;
        agente.speed = speed;
        animador.SetBool("isRunning", run);
        animador.SetBool("isWalking", walk);
    }

    private void DetenerZombi()
    {
        if (agente.isOnNavMesh) 
        {
            agente.isStopped = true;
        }
        animador.SetBool("isRunning", false);
        animador.SetBool("isWalking", false);
    }

    // --- ESTADOS DE LA IA ---

    private void EstadoPersecucion()
    {
        float distanciaSqr = (transform.position - jugador.position).sqrMagnitude;
        
        if (atacanteActual != null && atacanteActual != this && atacanteActual.gameObject.activeInHierarchy)
        {
            ControlarMovimiento(velocidadCaminar, false, true); // Camina mientras merodea

            if (!agente.hasPath || agente.remainingDistance < 1.0f)
            {
                Vector3 dirDesdeJugador = (transform.position - jugador.position).normalized;
                if (dirDesdeJugador == Vector3.zero) dirDesdeJugador = transform.forward;
                Vector3 variacion = Vector3.Cross(dirDesdeJugador, Vector3.up) * Random.Range(-0.8f, 0.8f);
                Vector3 dirFinal = (dirDesdeJugador + variacion).normalized;
                float radioEspera = Random.Range(3.0f, 6.0f);
                Vector3 destino = jugador.position + dirFinal * radioEspera;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(destino, out hit, 4.0f, NavMesh.AllAreas))
                {
                    agente.SetDestination(hit.position);
                }
            }
            return;
        }

        if (distanciaSqr <= distanciaAtaqueSqr)
        {
            if (Time.time < tiempoUltimoAtaque + tiempoEntreAtaques)
            {
                DetenerZombi();
                transform.LookAt(jugador.position);
                return;
            }

            if (atacanteActual == null || atacanteActual == this || !atacanteActual.gameObject.activeInHierarchy)
            {
                atacanteActual = this;
                estadoActual = EstadoZombi.Atacando;
            }
            else
            {
                DetenerZombi();
                transform.LookAt(jugador.position);
            }
            return;
        }

        ControlarMovimiento(velocidadCorrer, true, false); // Corre al perseguir
        if (Time.frameCount % 5 == 0)
        {
            agente.SetDestination(jugador.position);
        }
    }

    private void EstadoAtaque()
    {
        if (jugador.CompareTag("Untagged"))
        {
            estadoActual = EstadoZombi.Persiguiendo;
            return;
        }

        DetenerZombi();
        if (jugador != null) transform.LookAt(jugador.position);

        if (Time.time > tiempoUltimoAtaque + tiempoEntreAtaques)
        {
            tiempoUltimoAtaque = Time.time;
            animador.SetTrigger("bite");
            
            if (GestorAudio.Instancia != null) GestorAudio.Instancia.ReproducirMordida();
            if (GestorJuego.Instancia != null) GestorJuego.Instancia.PerderVida();

            if (atacanteActual == this) atacanteActual = null;
            estadoActual = EstadoZombi.Retirandose;
            tiempoSiguienteEstado = Time.time + duracionRetirada;
        }
        else if (Vector3.Distance(transform.position, jugador.position) > distanciaAtaque)
        {
            if (atacanteActual == this) atacanteActual = null;
            estadoActual = EstadoZombi.Persiguiendo;
        }
    }

    public void InterrumpirAtaque()
    {
        if (atacanteActual == this) atacanteActual = null;
        estadoActual = EstadoZombi.Persiguiendo;
    }

    private void EstadoRetirada()
    {
        ControlarMovimiento(velocidadCorrer, true, false);
        Vector3 lejosDelJugador = transform.position - jugador.position;
        agente.SetDestination(transform.position + lejosDelJugador.normalized * 2f);

        if (Time.time > tiempoSiguienteEstado)
        {
            estadoActual = EstadoZombi.Persiguiendo;
        }
    }

    private void EstadoDeambulacion()
    {
        ControlarMovimiento(velocidadCaminar, false, true);
        if (!agente.pathPending && agente.remainingDistance <= agente.stoppingDistance)
        {
            BuscarDestinoAleatorio();
        }
    }

    private void BuscarDestinoAleatorio()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas))
        {
            agente.SetDestination(hit.position);
        }
    }

    public void RecibirDano(int dano)
    {
        if (estaMuerto) return;
        saludActual -= dano;
        if (saludActual <= 0) AlMorir();
    }

    void AlMorir()
    {
        if (estaMuerto) return;
        estaMuerto = true;
        if (atacanteActual == this) atacanteActual = null;

        if (GestorAudio.Instancia != null) GestorAudio.Instancia.ReproducirSonidoMuerteZombi(transform.position);
        
        DetenerZombi();
        agente.enabled = false;
        GetComponent<Collider>().enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        animador.SetBool("die", true);
        StartCoroutine(EsperarYDesaparecer());
    }

    IEnumerator EsperarYDesaparecer()
    {
        yield return new WaitForSeconds(3f);
        if (miGenerador != null) miGenerador.ZombieMuerto(gameObject);
        else gameObject.SetActive(false);
    }

    void ProcesarSonidos()
    {
        if (Time.time > tiempoSiguienteGemido && !estaMuerto)
        {
            AudioClip clip = null;
            float distSqr = (jugador != null) ? (transform.position - jugador.position).sqrMagnitude : float.MaxValue;

            if (distSqr < 25f && sonidosCerca != null && sonidosCerca.Length > 0)
                clip = sonidosCerca[Random.Range(0, sonidosCerca.Length)];
            else if (sonidosAmbiente != null && sonidosAmbiente.Length > 0)
                clip = sonidosAmbiente[Random.Range(0, sonidosAmbiente.Length)];

            if (clip != null) audioSourcePropio.PlayOneShot(clip, 0.6f);
            tiempoSiguienteGemido = Time.time + Random.Range(4f, 12f);
        }

        if (agente.velocity.sqrMagnitude > 0.2f && !estaMuerto && Time.time > tiempoSiguientePaso)
        {
            if (sonidosPasos.Length > 0) audioSourcePropio.PlayOneShot(sonidosPasos[Random.Range(0, sonidosPasos.Length)], 0.6f);
            float intervalo = estadoActual == EstadoZombi.Deambulando ? 0.6f : 0.35f;
            tiempoSiguientePaso = Time.time + intervalo;
        }
    }
}