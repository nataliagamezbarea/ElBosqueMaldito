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
        else
        {
            Debug.LogWarning("LimitesJugador: No se encontró un terreno activo. El script se desactivará.");
            enabled = false; // Desactiva el método Update() si no hay terreno.
        }
    }

    void Update()
    {
        Vector3 posicionJugador = transform.position;

        posicionJugador.x = Mathf.Clamp(posicionJugador.x, posicionTerreno.x, posicionTerreno.x + tamanoTerreno.x);
        posicionJugador.z = Mathf.Clamp(posicionJugador.z, posicionTerreno.z, posicionTerreno.z + tamanoTerreno.z);

        transform.position = posicionJugador;
    }
}
