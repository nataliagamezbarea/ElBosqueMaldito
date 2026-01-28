using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class GestorPools : MonoBehaviour
{
    public static GestorPools Instancia;

    [System.Serializable]
    public class Grupo
    {
        public string etiqueta;
        public GameObject prefab;
        public int tamano;
    }

    public List<Grupo> grupos;
    public Dictionary<string, Queue<GameObject>> diccionarioGrupos;

    void Awake()
    {
        Instancia = this;
        diccionarioGrupos = new Dictionary<string, Queue<GameObject>>();

        foreach (Grupo grupo in grupos)
        {
            Queue<GameObject> colaObjetos = new Queue<GameObject>();

            for (int i = 0; i < grupo.tamano; i++)
            {
                GameObject obj = Instantiate(grupo.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                colaObjetos.Enqueue(obj);
            }

            diccionarioGrupos.Add(grupo.etiqueta, colaObjetos);
        }
    }

    public GameObject GenerarDesdeGrupo(string etiqueta, Vector3 posicion, Quaternion rotacion)
    {
        if (!diccionarioGrupos.ContainsKey(etiqueta))
        {
            Debug.LogWarning("Grupo con etiqueta " + etiqueta + " no existe.");
            return null;
        }

        var cola = diccionarioGrupos[etiqueta];
        while (cola.Count > 0 && cola.Peek() == null)
        {
            cola.Dequeue();
        }

        if (cola.Count == 0 || cola.Peek().activeInHierarchy)
        {
            ExpandirGrupo(etiqueta);
        }

        GameObject objetoAGenerar = cola.Dequeue();

        objetoAGenerar.transform.position = posicion;
        objetoAGenerar.transform.rotation = rotacion;
        objetoAGenerar.SetActive(true);

        return objetoAGenerar;
    }

    private void ExpandirGrupo(string etiqueta)
    {
        Grupo grupo = grupos.Find(p => p.etiqueta == etiqueta);
        if (grupo != null)
        {
            GameObject obj = Instantiate(grupo.prefab);
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            diccionarioGrupos[etiqueta].Enqueue(obj);
        }
    }

    public void AsegurarTamanoGrupo(string etiqueta, int tamanoRequerido)
    {
        if (diccionarioGrupos.ContainsKey(etiqueta))
        {
            int tamanoActual = diccionarioGrupos[etiqueta].Count;
            if (tamanoActual < tamanoRequerido)
            {
                int paraAnadir = tamanoRequerido - tamanoActual;
                for (int i = 0; i < paraAnadir; i++)
                {
                    ExpandirGrupo(etiqueta);
                }
            }
        }
        else
        {
            Debug.LogWarning($"PoolManager: No se encontró el grupo con etiqueta '{etiqueta}'. Asegúrate de configurarlo en el Inspector con tamaño 1 al menos.");
        }
    }

    public void RegistrarGrupo(string etiqueta, GameObject prefab, int tamano)
    {
        if (diccionarioGrupos.ContainsKey(etiqueta)) return;

        Grupo nuevoGrupo = new Grupo { etiqueta = etiqueta, prefab = prefab, tamano = tamano };
        grupos.Add(nuevoGrupo);

        Queue<GameObject> colaObjetos = new Queue<GameObject>();

        for (int i = 0; i < tamano; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            colaObjetos.Enqueue(obj);
        }

        diccionarioGrupos.Add(etiqueta, colaObjetos);
    }

    public void DevolverAGrupo(string etiqueta, GameObject obj)
    {
        obj.SetActive(false);

        if (!diccionarioGrupos.ContainsKey(etiqueta))
        {
            Debug.LogWarning($"Grupo con etiqueta '{etiqueta}' no existe. El objeto será destruido.");
            Destroy(obj);
            return;
        }

        diccionarioGrupos[etiqueta].Enqueue(obj);
    }
}