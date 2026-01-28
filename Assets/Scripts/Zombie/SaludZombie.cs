using UnityEngine;

public class SaludZombi : MonoBehaviour
{
    public int salud = 30;

    public void RecibirDano(int cantidad)
    {
        salud -= cantidad;
        if (salud <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        GestorJuego.Instancia.SumarMuerteZombi();
        Destroy(gameObject);
    }
}
