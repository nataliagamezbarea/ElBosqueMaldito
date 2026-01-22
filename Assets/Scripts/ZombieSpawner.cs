using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject[] zombiePrefabs;
    
    [Header("Configuración de Oleadas")]
    public int maxZombiesAlMismoTiempo = 30; // Límite para evitar lag
    public float tiempoEntreSpawns = 5f;    // Aparece uno cada 5 segundos

    [Header("Cámaras")]
    public Camera firstPersonCam;
    public Camera thirdPersonCam;

    [Header("Rango de Aparición")]
    public float minDistance = 15f; 
    public float maxDistance = 40f; 

    private Transform player;
    private int zombiesActuales = 0;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Iniciamos la rutina de aparición infinita
        StartCoroutine(RutinaSpawnInfinita());
    }

    IEnumerator RutinaSpawnInfinita()
    {
        while (true) // Bucle infinito
        {
            // Solo spawnea si no hemos llegado al límite
            if (zombiesActuales < maxZombiesAlMismoTiempo)
            {
                SpawnZombie();
            }

            // Espera el tiempo configurado antes de intentar el siguiente
            yield return new WaitForSeconds(tiempoEntreSpawns);
        }
    }

    void SpawnZombie()
    {
        Camera activeCam = GetActiveCamera();
        Camera camToUse = activeCam != null ? activeCam : Camera.main;

        Vector3 spawnPos = Vector3.zero;
        bool validPoint = false;

        for (int i = 0; i < 30; i++) 
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minDistance, maxDistance);
            Vector3 candidatePoint = new Vector3(player.position.x + randomCircle.x, player.position.y, player.position.z + randomCircle.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidatePoint, out hit, 10f, NavMesh.AllAreas))
            {
                Vector3 screenPoint = camToUse.WorldToViewportPoint(hit.position);

                // Verificamos que esté fuera de cámara
                bool isOffScreen = screenPoint.z < 0 || screenPoint.x < -0.05f || screenPoint.x > 1.05f || screenPoint.y < -0.05f || screenPoint.y > 1.05f;

                if (isOffScreen)
                {
                    spawnPos = hit.position;
                    validPoint = true;
                    break;
                }
            }
        }

        if (validPoint)
        {
            GameObject prefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];
            GameObject nuevoZombie = Instantiate(prefab, spawnPos, Quaternion.identity);
            
            zombiesActuales++;
            
            // OPCIONAL: Si el zombie muere, avisar al spawner para que cree otro
            // Para esto necesitarías un script de salud en el zombie que llame a ReducirContador()
        }
    }

    Camera GetActiveCamera()
    {
        if (firstPersonCam != null && firstPersonCam.gameObject.activeInHierarchy) return firstPersonCam;
        if (thirdPersonCam != null && thirdPersonCam.gameObject.activeInHierarchy) return thirdPersonCam;
        return null;
    }

    // Método para llamar cuando un zombie muera
    public void ZombieMuerto()
    {
        zombiesActuales--;
    }
}