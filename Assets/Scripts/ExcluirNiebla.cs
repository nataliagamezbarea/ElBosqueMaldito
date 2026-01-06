using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class ExcluirNieblaRenderTexture : MonoBehaviour
{
    private Camera m_Camera;
    private bool fogOriginalState;

    void OnEnable()
    {
        m_Camera = GetComponent<Camera>();
        RenderPipelineManager.beginCameraRendering += OnBeginCamera;
        RenderPipelineManager.endCameraRendering += OnEndCamera;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
        RenderPipelineManager.endCameraRendering -= OnEndCamera;
    }

    void OnBeginCamera(ScriptableRenderContext context, Camera camera)
    {
        // Verificamos que sea esta cámara, incluso si renderiza a una textura
        if (camera == m_Camera)
        {
            fogOriginalState = RenderSettings.fog;
            RenderSettings.fog = false; // Desactivamos la niebla global
        }
    }

    void OnEndCamera(ScriptableRenderContext context, Camera camera)
    {
        if (camera == m_Camera)
        {
            RenderSettings.fog = fogOriginalState; // Restauramos para la Main Camera
        }
    }
}