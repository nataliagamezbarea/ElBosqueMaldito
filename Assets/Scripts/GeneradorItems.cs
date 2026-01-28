using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class GeneradorItems : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject prefabVida;
    public GameObject prefabEscudo;
    public int maxItemsSimultaneos = 1;
    public float intervaloGeneracion = 3f;
    public float alturaSpawn = 1f; // Altura extra sobre el suelo

    [Header("Generación Aleatoria")]
    public float radioMinimo = 5f;
    public float radioMaximo = 20f;
    private SaludJugador saludJugador; // Se buscará automáticamente

    // Pools
    private List<GameObject> poolVidas = new List<GameObject>();
    private List<GameObject> poolEscudos = new List<GameObject>();
    
    private float cronometro;

    private void Start()
    {
        // AUTOMATIZACIÓN: Buscamos al jugador por su Tag en lugar de arrastrarlo
        GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");
        if (objetoJugador != null)
        {
            saludJugador = objetoJugador.GetComponent<SaludJugador>();
        }

        // Inicializamos los pools creando los objetos desactivados
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
        // SEGURIDAD: Si por alguna razón perdimos al jugador (ej. reinicio), lo buscamos de nuevo
        if (saludJugador == null)
        {
            GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");
            if (objetoJugador != null) saludJugador = objetoJugador.GetComponent<SaludJugador>();
        }

        // RESTRICCIÓN: Si ya hay algún ítem activo (sea vida o escudo), no generamos nada más
        if (HayItemActivo(poolVidas) || HayItemActivo(poolEscudos)) return;

        // Decidir aleatoriamente qué generar (50% probabilidad)
        bool generarVida = Random.value > 0.5f;

        if (generarVida)
        {
            // LÓGICA CLAVE: Solo generamos vida si el jugador NO tiene la salud completa
            // Asumimos que SaludJugador tiene variables públicas 'saludActual' y 'saludMaxima'
            // Si tus variables son privadas, necesitarás crear un método público bool TieneSaludCompleta()
            if (saludJugador != null && saludJugador.saludActual < saludJugador.saludMaxima)
            {
                ActivarItemDelPool(poolVidas, prefabVida);
            }
            else
            {
                // MEJORA: Si la vida está llena, generamos un escudo en su lugar para no perder el turno
                ActivarItemDelPool(poolEscudos, prefabEscudo);
            }
        }
        else
        {
            // El escudo se puede generar siempre
            ActivarItemDelPool(poolEscudos, prefabEscudo);
        }
    }

    private void ActivarItemDelPool(List<GameObject> pool, GameObject prefabReferencia)
    {
        // Buscar un objeto inactivo en el pool
        foreach (GameObject item in pool)
        {
            if (!item.activeInHierarchy)
            {
                Vector3 destino = Vector3.zero;
                bool destinoCalculado = false;

                if (saludJugador != null)
                {
                    Vector3 posicionJugador = saludJugador.transform.position;
                    // 1. LÓGICA MEJORADA: Buscamos el zombi más cercano y ponemos el ítem ENTRE él y tú
                    GameObject[] zombies = GameObject.FindGameObjectsWithTag("Enemy");
                    
                    if (zombies.Length > 0)
                    {
                        GameObject zombieMasCercano = null;
                        float distMinSqr = float.MaxValue;
                        
                        // Encontrar al más cercano
                        foreach(var z in zombies) {
                            float dSqr = (posicionJugador - z.transform.position).sqrMagnitude;
                            if(dSqr < distMinSqr) { distMinSqr = dSqr; zombieMasCercano = z; }
                        }

                        if (zombieMasCercano != null)
                        {
                            Vector3 dir = (zombieMasCercano.transform.position - posicionJugador).normalized;
                            // Distancia: Mitad de camino, pero MÁXIMO 8 metros para que esté accesible
                            float distReal = Mathf.Sqrt(distMinSqr);
                            float distancia = Mathf.Clamp(distReal * 0.5f, 2f, 8f);
                            // Añadimos un poco de desviación lateral para que no sea una línea recta perfecta
                            Vector3 lateral = Vector3.Cross(dir, Vector3.up) * Random.Range(-2f, 2f);
                            
                            destino = posicionJugador + (dir * distancia) + lateral;
                            destinoCalculado = true;
                        }
                    }

                    // 2. Fallback: Si no hay zombis, aleatorio alrededor del jugador
                    if (!destinoCalculado)
                    {
                        Vector2 circulo = Random.insideUnitCircle.normalized * Random.Range(radioMinimo, radioMaximo);
                        destino = posicionJugador + new Vector3(circulo.x, 0, circulo.y);
                    }

                    if (NavMesh.SamplePosition(destino, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                    {
                        item.transform.position = hit.position + Vector3.up * alturaSpawn;
                        item.transform.rotation = prefabReferencia.transform.rotation; // Usamos la rotación del prefab
                        item.transform.localScale = prefabReferencia.transform.localScale; // Restauramos la escala original
                        item.SetActive(true);
                    }
                }
                return; // Salimos tras activar uno solo
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
