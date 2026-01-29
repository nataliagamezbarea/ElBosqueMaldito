using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class GeneradorZombis : MonoBehaviour
{
    [Tooltip("Arrastra aquí todos los tipos de zombis que quieras. Se añadirán al Pool automáticamente.")]
    public List<GameObject> prefabsZombis;
    
    [Header("Límites de Población")]
    public int maxZombiesTotales = 10; 
    public int maxZombisSimultaneos = 10;
    public float tiempoChequeo = 2f; 

    [Header("Dificultad Progresiva")]
    public float velocidadInicial = 3.5f;
    public float aumentoPorBaja = 0.1f;
    public float velocidadMaxima = 7.0f;

    [Header("Rango de Aparición")]
    public float distanciaMinima = 20f; 
    public float distanciaMaxima = 40f; 

    private Transform jugador;
    private float velocidadActual;
    
    private int contadorZombisActivos = 0;
    private int zombisGenerados = 0;

    private List<GameObject> listaZombisActivos = new List<GameObject>();
    private bool modoEscudoActivo = false;

    IEnumerator Start() {
        velocidadActual = velocidadInicial;
        
        GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");
        if (objetoJugador != null) jugador = objetoJugador.transform;
        else Debug.LogWarning("ZombieSpawner: No se encontró el objeto Player.");

        if (prefabsZombis == null || prefabsZombis.Count == 0)
        {
            Debug.LogError("ZombieSpawner: ¡CUIDADO! La lista 'Zombie Prefabs' está vacía. Arrastra tus prefabs de zombis en el Inspector.");
            yield break;
        }
        
        while (GestorPools.Instancia == null)
        {
            yield return null;
        }
        
        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.DefinirTotalZombis(maxZombiesTotales);
        }

        foreach (var prefab in prefabsZombis)
        {
            if (prefab != null)
            {
                GestorPools.Instancia.RegistrarGrupo(prefab.name, prefab, maxZombiesTotales);
            }
        }

        // Capturar zombies que ya existan en la escena (por si se colocaron manualmente)
        ControladorZombi[] existentes = FindObjectsOfType<ControladorZombi>();
        foreach (var z in existentes) {
            if (!listaZombisActivos.Contains(z.gameObject)) {
                listaZombisActivos.Add(z.gameObject);
            }
        }

        StartCoroutine(BucleDeSpawn());
    }

    void Update() {
        if (jugador == null) {
            // Intentar recuperar al jugador si se perdió o si empezó con el tag Untagged
            GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");
            if (objetoJugador == null) objetoJugador = GameObject.FindGameObjectWithTag("Untagged");
            
            if (objetoJugador != null) jugador = objetoJugador.transform;
            else return;
        }

        bool tieneEscudo = jugador.CompareTag("Untagged");

        if (tieneEscudo != modoEscudoActivo) {
            modoEscudoActivo = tieneEscudo;
        }

        ActualizarMusicaTension();
    }

    void ActualizarMusicaTension()
    {
        if (GestorAudio.Instancia == null || jugador == null) return;

        float distanciaMinimaSqr = float.MaxValue;
        
        // Buscar el zombi más cercano
        foreach (var z in listaZombisActivos)
        {
            if (z != null && z.activeInHierarchy)
            {
                float d = (z.transform.position - jugador.position).sqrMagnitude;
                if (d < distanciaMinimaSqr) distanciaMinimaSqr = d;
            }
        }

        // Calcular intensidad (0 a 1). Si está a menos de 5m = 1.0, si está a más de 20m = 0.0
        float distanciaReal = Mathf.Sqrt(distanciaMinimaSqr);
        float intensidad = Mathf.Clamp01(1f - ((distanciaReal - 5f) / 15f));
        
        GestorAudio.Instancia.ActualizarTension(intensidad);
    }

    IEnumerator BucleDeSpawn() {
        while (true) {
            if (contadorZombisActivos < maxZombisSimultaneos && zombisGenerados < maxZombiesTotales) {
                GenerarZombi();
            }
            yield return new WaitForSeconds(tiempoChequeo);
        }
    }

    void GenerarZombi() {
        if (GestorPools.Instancia == null || prefabsZombis.Count == 0) return;
        if (jugador == null) return;

        // Si el jugador tiene el escudo (es "Untagged"), no generamos zombies cerca de él
        if (jugador != null && jugador.CompareTag("Untagged")) return;

        Vector2 circuloAleatorio = Random.insideUnitCircle.normalized * Random.Range(distanciaMinima, distanciaMaxima);
        Vector3 puntoCandidato = new Vector3(jugador.position.x + circuloAleatorio.x, jugador.position.y, jugador.position.z + circuloAleatorio.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(puntoCandidato, out hit, 5f, NavMesh.AllAreas)) {
            
            GameObject prefabElegido = prefabsZombis[Random.Range(0, prefabsZombis.Count)];
            string claveGrupo = prefabElegido.name;

            GameObject nuevoZombie = GestorPools.Instancia.GenerarDesdeGrupo(claveGrupo, hit.position, Quaternion.identity);
            
            if (nuevoZombie != null)
            {
                contadorZombisActivos++;
                zombisGenerados++;
                listaZombisActivos.Add(nuevoZombie);
                var controlador = nuevoZombie.GetComponent<ControladorZombi>();
                if (controlador != null) controlador.ReiniciarZombi(velocidadActual, this, claveGrupo);
            }
        }
    }

    public void ZombieMuerto(GameObject zombieQueMurio) {
        string claveGrupo = zombieQueMurio.GetComponent<ControladorZombi>().claveGrupo;
        
        GestorPools.Instancia.DevolverAGrupo(claveGrupo, zombieQueMurio);
        listaZombisActivos.Remove(zombieQueMurio);
        contadorZombisActivos--;
        
        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.SumarMuerteZombi();
            GestorJuego.Instancia.SumarPuntuacion(10);
        }

        if (velocidadActual < velocidadMaxima) {
            velocidadActual += aumentoPorBaja;
        }
    }
}