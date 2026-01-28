using UnityEngine;

public class SaludJugador : MonoBehaviour
{
    public int saludMaxima = 100;
    public int saludActual;

    void Start()
    {
        saludActual = saludMaxima;
    }

    public void RecibirDano(int dano)
    {
        saludActual -= dano;
        if (saludActual <= 0) 
        {
            if (GestorAudio.Instancia != null) GestorAudio.Instancia.ReproducirMuerteJugador();
            GestorJuego.Instancia.PerderVida();
        }
    }

    public void AnadirSalud(int cantidad)
    {
        if (cantidad > 0 && GestorAudio.Instancia != null) GestorAudio.Instancia.ReproducirPickupVida();
        
        saludActual += cantidad;
        if (saludActual > saludMaxima) saludActual = saludMaxima;
    }
}