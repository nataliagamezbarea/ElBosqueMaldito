using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class ExcluirNieblaRenderTexture : MonoBehaviour
{
    private Camera miCamara;
    private bool estadoOriginalNiebla;

    void OnEnable()
    {
        miCamara = GetComponent<Camera>();
        RenderPipelineManager.beginCameraRendering += AlEmpezarCamara;
        RenderPipelineManager.endCameraRendering += AlTerminarCamara;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= AlEmpezarCamara;
        RenderPipelineManager.endCameraRendering -= AlTerminarCamara;
    }

    private void AlEmpezarCamara(ScriptableRenderContext context, Camera camera)
    {
        if (camera.GetInstanceID() == miCamara.GetInstanceID())
        {
            estadoOriginalNiebla = RenderSettings.fog;
            if (estadoOriginalNiebla)
            {
                RenderSettings.fog = false;
            }
        }
    }

    private void AlTerminarCamara(ScriptableRenderContext context, Camera camera)
    {
        if (camera.GetInstanceID() == miCamara.GetInstanceID())
        {
            if (estadoOriginalNiebla)
            {
                RenderSettings.fog = true;
            }
        }
    }
}