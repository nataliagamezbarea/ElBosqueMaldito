using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject[] zombiePrefabs;
    
    [Header("Límites de Población")]
    public int maxZombiesTotales = 2; 
    public float tiempoChequeo = 6f; 

    [Header("Dificultad Progresiva")]
    public float velocidadInicial = 3.5f;
    public float aumentoPorBaja = 0.15f;
    public float velocidadMaxima = 7.5f;

    [Header("Rango de Aparición")]
    public float minDistance = 20f; 
    public float maxDistance = 40f; 

    private Transform player;
    private float velocidadActual;
    
    // Lista para rastrear qué prefabs están vivos actualmente
    private List<GameObject> prefabsEnEscena = new List<GameObject>();

    void Start() {
        velocidadActual = velocidadInicial;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(BucleDeSpawn());
    }

    IEnumerator BucleDeSpawn() {
        while (true) {
            if (prefabsEnEscena.Count < maxZombiesTotales) {
                SpawnZombie();
            }
            yield return new WaitForSeconds(tiempoChequeo);
        }
    }

    void SpawnZombie() {
        if (zombiePrefabs.Length < 2) {
            Debug.LogWarning("Necesitas al menos 2 prefabs diferentes en la lista para que no se repitan.");
            if (zombiePrefabs.Length == 0) return;
        }

        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minDistance, maxDistance);
        Vector3 candidatePoint = new Vector3(player.position.x + randomCircle.x, player.position.y, player.position.z + randomCircle.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(candidatePoint, out hit, 15f, NavMesh.AllAreas)) {
            
            GameObject prefabAElegir = null;

            // Lógica para elegir uno que NO esté en escena
            List<GameObject> opcionesDisponibles = new List<GameObject>(zombiePrefabs);
            
            // Eliminamos de las opciones los que ya están vivos (comparando por nombre o referencia)
            foreach (GameObject vivo in prefabsEnEscena) {
                opcionesDisponibles.RemoveAll(p => p.name == vivo.name.Replace("(Clone)", "").Trim());
            }

            if (opcionesDisponibles.Count > 0) {
                prefabAElegir = opcionesDisponibles[Random.Range(0, opcionesDisponibles.Count)];
            } else {
                // Si por alguna razón no hay opciones, elegimos uno al azar para no romper el spawn
                prefabAElegir = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];
            }

            GameObject nuevoZombie = Instantiate(prefabAElegir, hit.position, Quaternion.identity);
            prefabsEnEscena.Add(nuevoZombie);
            
            NavMeshAgent agent = nuevoZombie.GetComponent<NavMeshAgent>();
            if (agent != null) {
                agent.speed = velocidadActual;
            }
        }
    }

    public void ZombieMuerto(GameObject zombieQueMurio) {
        // Quitamos este zombie específico de la lista de seguimiento
        if (prefabsEnEscena.Contains(zombieQueMurio)) {
            prefabsEnEscena.Remove(zombieQueMurio);
        }
        
        if (velocidadActual < velocidadMaxima) {
            velocidadActual += aumentoPorBaja;
        }
    }
}