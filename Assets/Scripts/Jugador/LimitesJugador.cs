using UnityEngine;

public class LimitesJugador : MonoBehaviour
{
    public Terrain terreno;
    private Vector3 tamanoTerreno;
    private Vector3 posicionTerreno;

    void Start()
    {
        if (terreno == null)
        {
            terreno = Terrain.activeTerrain;
        }

        if (terreno != null)
        {
            tamanoTerreno = terreno.terrainData.size;
            posicionTerreno = terreno.transform.position;
        }
    }

    void Update()
    {
        if (terreno != null)
        {
            Vector3 posicionJugador = transform.position;

            posicionJugador.x = Mathf.Clamp(posicionJugador.x, posicionTerreno.x, posicionTerreno.x + tamanoTerreno.x);
            posicionJugador.z = Mathf.Clamp(posicionJugador.z, posicionTerreno.z, posicionTerreno.z + tamanoTerreno.z);

            transform.position = posicionJugador;
        }
    }
}
