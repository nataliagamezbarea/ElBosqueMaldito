﻿﻿﻿﻿﻿﻿﻿using UnityEngine;
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
        // Si el jugador tiene el escudo (Tag Untagged), el zombie lo pierde de vista
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

    private void DetenerZombi()
    {
        if (agente.isOnNavMesh) 
        {
            agente.ResetPath(); // Borra el destino guardado (el zombie "olvida")
            agente.isStopped = true;
        }
        animador.SetBool("isRunning", false);
    }

    private void EstadoPersecucion()
    {
        // OPTIMIZACIÓN: sqrMagnitude es mucho más rápido que Distance
        float distanciaSqr = (transform.position - jugador.position).sqrMagnitude;
        
        // MODIFICACIÓN: Si ya hay un atacante activo y no soy yo, merodeo alrededor sin parar.
        if (atacanteActual != null && atacanteActual != this && atacanteActual.gameObject.activeInHierarchy)
        {
            agente.speed = velocidadCaminar; // Se mueven despacio al merodear
            agente.isStopped = false;
            animador.SetBool("isRunning", true);

            // Si llegamos al destino de espera o no tenemos uno, buscamos otro punto alrededor
            if (!agente.hasPath || agente.remainingDistance < 1.0f)
            {
                // Calculamos una posición en un círculo alrededor del jugador (distancia 3 a 6 metros)
                Vector3 dirDesdeJugador = (transform.position - jugador.position).normalized;
                if (dirDesdeJugador == Vector3.zero) dirDesdeJugador = transform.forward;
                
                // Variación aleatoria lateral para que se muevan alrededor sin cruzar por el medio
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

        agente.speed = velocidadCorrer; // Corren al perseguir

        if (distanciaSqr <= distanciaAtaqueSqr)
        {
            float distancia = Mathf.Sqrt(distanciaSqr); // Solo calculamos la raíz si estamos cerca
            if (Time.time < tiempoUltimoAtaque + tiempoEntreAtaques)
            {
                agente.isStopped = true;
                animador.SetBool("isRunning", false);
                transform.LookAt(jugador.position);
                return;
            }

            bool puedoAtacar = false;

            if (atacanteActual == null)
            {
                puedoAtacar = true;
            }
            else if (atacanteActual != this)
            {
                if (!atacanteActual.gameObject.activeInHierarchy)
                {
                    puedoAtacar = true;
                }
                else
                {
                    float distanciaOtro = Vector3.Distance(atacanteActual.transform.position, jugador.position);
                    if (distancia < distanciaOtro - 0.2f)
                    {
                        atacanteActual.InterrumpirAtaque();
                        puedoAtacar = true;
                    }
                }
                // ELIMINADO: Ya no interrumpimos al atacante actual aunque estemos más cerca.
                // Esto asegura que sea un 1 contra 1 estricto.
            }

            if (puedoAtacar)
            {
                atacanteActual = this;
                estadoActual = EstadoZombi.Atacando;
            }
            else
            {
                agente.isStopped = true;
                animador.SetBool("isRunning", false);
                transform.LookAt(jugador.position);
            }
            return;
        }

        agente.isStopped = false;
        animador.SetBool("isRunning", true);

        if (Time.frameCount % 5 == 0)
        {
            agente.SetDestination(jugador.position);
        }
    }

    private void EstadoAtaque()
    {
        // Doble verificación: si de pronto el jugador activa el escudo en medio de la animación de ataque
        if (jugador.CompareTag("Untagged"))
        {
            estadoActual = EstadoZombi.Persiguiendo;
            return;
        }

        agente.isStopped = true;
        animador.SetBool("isRunning", false);
        if (jugador != null) transform.LookAt(jugador.position);

        if (Time.time > tiempoUltimoAtaque + tiempoEntreAtaques)
        {
            tiempoUltimoAtaque = Time.time;
            animador.SetTrigger("bite");
            
            if (GestorAudio.Instancia != null) 
            {
                GestorAudio.Instancia.ReproducirMordida();
            }
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
        agente.isStopped = false;
        animador.SetBool("isRunning", true);

        Vector3 lejosDelJugador = transform.position - jugador.position;
        agente.SetDestination(transform.position + lejosDelJugador.normalized * 2f);

        if (Time.time > tiempoSiguienteEstado)
        {
            estadoActual = EstadoZombi.Persiguiendo;
        }
    }

    private void EstadoDeambulacion()
    {
        agente.speed = velocidadCaminar; // Caminan si el jugador tiene escudo
        // Aseguramos que la animación sea la misma que al perseguir (isRunning)
        if (agente.isStopped) agente.isStopped = false;
        animador.SetBool("isRunning", true);

        if (!agente.pathPending && agente.remainingDistance <= agente.stoppingDistance)
        {
            BuscarDestinoAleatorio();
        }
    }

    private void BuscarDestinoAleatorio()
    {
        agente.isStopped = false;
        animador.SetBool("isRunning", true); // Activa la animación de caminar/correr
        
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
        if (estaMuerto) return; // Evita que muera múltiples veces si recibe mucho daño rápido

        saludActual -= dano;
        if (saludActual <= 0)
        {
            AlMorir();
        }
    }

    void AlMorir()
    {
        if (estaMuerto) return;
        estaMuerto = true;

        // Liberamos el turno de ataque inmediatamente al morir para que venga el siguiente
        if (atacanteActual == this) atacanteActual = null;

        if (GestorAudio.Instancia != null)
        {
            GestorAudio.Instancia.ReproducirSonidoMuerteZombi(transform.position);
        }
        
        if (agente != null)
        {
            if (agente.isOnNavMesh) agente.ResetPath();
            agente.enabled = false;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Restauramos esto: Al quitar el collider, si tiene Rigidbody y gravedad, se cae al vacío.
        // Lo ponemos kinematic para que se quede en el sitio reproduciendo la animación.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (animador != null) animador.SetBool("die", true);

        StartCoroutine(EsperarYDesaparecer());
    }

    IEnumerator EsperarYDesaparecer()
    {
        yield return new WaitForSeconds(3f);

        if (miGenerador != null) 
        {
            miGenerador.ZombieMuerto(gameObject);
        }
        else 
            gameObject.SetActive(false);
    }

    void ProcesarSonidos()
    {
        // Sonidos de ambiente (gemidos aleatorios)
        if (Time.time > tiempoSiguienteGemido)
        {
            if (!estaMuerto)
            {
                AudioClip clip = null;
                float distSqr = (jugador != null) ? (transform.position - jugador.position).sqrMagnitude : float.MaxValue;

                // Si está a menos de 5 metros (25 sqr) y tenemos sonidos de cerca
                if (distSqr < 25f && sonidosCerca != null && sonidosCerca.Length > 0)
                    clip = sonidosCerca[Random.Range(0, sonidosCerca.Length)];
                else if (sonidosAmbiente != null && sonidosAmbiente.Length > 0)
                    clip = sonidosAmbiente[Random.Range(0, sonidosAmbiente.Length)];

                if (clip != null) audioSourcePropio.PlayOneShot(clip);
            }
            tiempoSiguienteGemido = Time.time + Random.Range(4f, 12f);
        }

        // Sonidos de pasos (si se está moviendo)
        if (agente.velocity.sqrMagnitude > 0.2f && !estaMuerto)
        {
            if (Time.time > tiempoSiguientePaso)
            {
                if (sonidosPasos.Length > 0) audioSourcePropio.PlayOneShot(sonidosPasos[Random.Range(0, sonidosPasos.Length)], 0.6f);
                // Ajustar frecuencia de pasos según velocidad
                float intervalo = estadoActual == EstadoZombi.Deambulando ? 0.6f : 0.35f;
                tiempoSiguientePaso = Time.time + intervalo;
            }
        }
    }
}