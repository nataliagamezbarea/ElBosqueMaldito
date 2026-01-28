using System.Collections;
using UnityEngine;

public class HabilidadesJugador : MonoBehaviour
{
    public bool tieneEscudo { get; private set; } = false;

    public void ActivarEscudo(float duracion)
    {
        if (tieneEscudo) return;
        
        StartCoroutine(RutinaEscudo(duracion));
    }

    private IEnumerator RutinaEscudo(float duracion)
    {
        tieneEscudo = true;
        Debug.Log("¡Escudo activado! Eres indetectable e invulnerable por " + duracion + " segundos.");

        yield return new WaitForSeconds(duracion);

        tieneEscudo = false;
        Debug.Log("El escudo se ha desactivado.");
    }
}
