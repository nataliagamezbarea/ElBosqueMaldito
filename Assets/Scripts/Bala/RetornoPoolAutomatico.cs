using UnityEngine;

public class RetornoPoolAutomatico : MonoBehaviour
{
    public float tiempoVida = 2f;
    [HideInInspector] public string nombreGrupo;

    void OnEnable()
    {
        foreach (var ps in GetComponentsInChildren<ParticleSystem>())
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play(true);
        }

        Invoke(nameof(VolverAlPool), tiempoVida);
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void VolverAlPool()
    {
        if (GestorPools.Instancia != null && !string.IsNullOrEmpty(nombreGrupo))
        {
            GestorPools.Instancia.DevolverAGrupo(nombreGrupo, gameObject);
        }
        else
        {
            if (GestorPools.Instancia == null) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
