using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class GeneradorItems : MonoBehaviour
{
    [Header("Configuración General")]
    public GameObject prefabVida;
    public GameObject prefabEscudo;
    public int maxItemsSimultaneos = 1;
    public float intervaloGeneracion = 3f;
    public float alturaSpawn = 1f;

    [Header("Estrategia de Spawn")]
    [Tooltip("La capa en la que se encuentran los zombis para la búsqueda optimizada.")]
    public LayerMask capaZombi;
    [Tooltip("Radio máximo para buscar zombis cercanos para el spawn estratégico.")]
    public float radioBusquedaZombies = 40f;
    
    [Header("Configuración Vida")]
    public float radioMinimoVida = 5f;
    public float radioMaximoVida = 20f;
    public bool spawnEstrategicoVida = true;

    [Header("Configuración Escudo")]
    public float radioMinimoEscudo = 10f;
    public float radioMaximoEscudo = 30f;
    public bool spawnEstrategicoEscudo = false;

    private SaludJugador saludJugador;
    private List<GameObject> poolVidas = new List<GameObject>();
    private List<GameObject> poolEscudos = new List<GameObject>();
    private float cronometro;
    private Collider[] colliders = new Collider[50]; // Array pre-alocado para no generar basura

    private void Start()
    {
        GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");
        if (objetoJugador != null)
        {
            saludJugador = objetoJugador.GetComponent<SaludJugador>();
        }

        InicializarPool(poolVidas, prefabVida, maxItemsSimultaneos);
        InicializarPool(poolEscudos, prefabEscudo, maxItemsSimultaneos);
    }

    private void InicializarPool(List<GameObject> pool, GameObject prefab, int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    private void Update()
    {
        cronometro += Time.deltaTime;

        if (cronometro >= intervaloGeneracion)
        {
            IntentarGenerarItem();
            cronometro = 0f;
        }
    }

    private void IntentarGenerarItem()
    {
        if (saludJugador == null)
        {
            GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");
            if (objetoJugador != null) saludJugador = objetoJugador.GetComponent<SaludJugador>();
        }

        if (HayItemActivo(poolVidas) || HayItemActivo(poolEscudos)) return;

        bool generarVida = Random.value > 0.5f;

        if (generarVida)
        {
            if (saludJugador != null && saludJugador.saludActual < saludJugador.saludMaxima)
            {
                ActivarItemDelPool(poolVidas, prefabVida, radioMinimoVida, radioMaximoVida, spawnEstrategicoVida);
            }
            else
            {
                ActivarItemDelPool(poolEscudos, prefabEscudo, radioMinimoEscudo, radioMaximoEscudo, spawnEstrategicoEscudo);
            }
        }
        else
        {
            ActivarItemDelPool(poolEscudos, prefabEscudo, radioMinimoEscudo, radioMaximoEscudo, spawnEstrategicoEscudo);
        }
    }

    private void ActivarItemDelPool(List<GameObject> pool, GameObject prefabReferencia, float minRadio, float maxRadio, bool usarEstrategia)
    {
        foreach (GameObject item in pool)
        {
            if (!item.activeInHierarchy)
            {
                Vector3 destino = Vector3.zero;
                bool destinoCalculado = false;

                if (saludJugador != null)
                {
                    Vector3 posicionJugador = saludJugador.transform.position;
                    
                    int numColliders = Physics.OverlapSphereNonAlloc(posicionJugador, radioBusquedaZombies, colliders, capaZombi);

                    if (usarEstrategia && numColliders > 0)
                    {
                        Transform zombieMasCercano = null;
                        float distMinSqr = float.MaxValue;

                        for (int i = 0; i < numColliders; i++)
                        {
                            float dSqr = (posicionJugador - colliders[i].transform.position).sqrMagnitude;
                            if (dSqr < distMinSqr)
                            {
                                distMinSqr = dSqr;
                                zombieMasCercano = colliders[i].transform;
                            }
                        }

                        if (zombieMasCercano != null)
                        {
                            Vector3 dir = (zombieMasCercano.position - posicionJugador).normalized;
                            float distReal = Mathf.Sqrt(distMinSqr);
                            float distancia = Mathf.Clamp(distReal * 0.5f, 2f, 8f);
                            Vector3 lateral = Vector3.Cross(dir, Vector3.up) * Random.Range(-2f, 2f);
                            
                            destino = posicionJugador + (dir * distancia) + lateral;
                            destinoCalculado = true;
                        }
                    }

                    if (!destinoCalculado)
                    {
                        Vector2 circulo = Random.insideUnitCircle.normalized * Random.Range(minRadio, maxRadio);
                        destino = posicionJugador + new Vector3(circulo.x, 0, circulo.y);
                    }

                    if (NavMesh.SamplePosition(destino, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                    {
                        item.transform.position = hit.position + Vector3.up * alturaSpawn;
                        item.transform.rotation = prefabReferencia.transform.rotation;
                        item.transform.localScale = prefabReferencia.transform.localScale;
                        item.SetActive(true);
                    }
                }
                return;
            }
        }
    }

    private bool HayItemActivo(List<GameObject> pool)
    {
        foreach (GameObject item in pool)
        {
            if (item.activeInHierarchy) return true;
        }
        return false;
    }
}
