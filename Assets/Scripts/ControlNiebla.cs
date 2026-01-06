using UnityEngine;

[ExecuteAlways]
public class ControlNiebla : MonoBehaviour
{
    [Header("Configuración de Niebla")]
    public bool activarNiebla = true;
    public Color colorNiebla = new Color(0.263f, 0.329f, 0.431f); 
    public float densidad = 0.019f; 

    void Update()
    {
        RenderSettings.fog = activarNiebla;
        RenderSettings.fogColor = colorNiebla;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = densidad;
    }

    void OnDisable()
    {
        RenderSettings.fog = false;
    }
}