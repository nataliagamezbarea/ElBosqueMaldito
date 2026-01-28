using UnityEngine;

[ExecuteAlways]
public class ControlNiebla : MonoBehaviour
{
    [Header("Configuración de Niebla")]
    [SerializeField] private bool activarNiebla = true;
    [SerializeField] private Color colorNiebla = new Color(0.263f, 0.329f, 0.431f);
    [SerializeField] private float densidad = 0.019f;

    private void OnValidate()
    {
        ActualizarConfiguracion();
    }

    private void Awake()
    {
        ActualizarConfiguracion();
    }

    public void ActualizarConfiguracion()
    {
        if (RenderSettings.fog != activarNiebla)
            RenderSettings.fog = activarNiebla;

        RenderSettings.fogColor = colorNiebla;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = densidad;
    }

    private void OnDisable()
    {
        RenderSettings.fog = false;
    }
}