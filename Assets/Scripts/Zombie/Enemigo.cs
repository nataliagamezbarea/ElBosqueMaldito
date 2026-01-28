using UnityEngine;

public class Enemigo : MonoBehaviour
{
    public float salud = 50f;

    public void RecibirDano(float cantidad)
    {
        salud -= cantidad;
        if (salud <= 0f)
        {
            Morir();
        }
    }

    void Morir()
    {
        if (GestorJuego.Instancia != null) GestorJuego.Instancia.SumarMuerteZombi();
        Destroy(gameObject);
    }
}