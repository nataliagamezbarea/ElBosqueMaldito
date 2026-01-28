using UnityEngine;

public class PotenciadorSalud : MonoBehaviour
{
    public int saludAOtorgar = 1;

    private void OnTriggerEnter(Collider other)
    {
        // Usamos GetComponentInParent para encontrar al jugador aunque su tag sea "Untagged" por el escudo
        SaludJugador saludJugador = other.GetComponentInParent<SaludJugador>();

        if (saludJugador != null)
        {
            saludJugador.AnadirSalud(saludAOtorgar);
            // En lugar de Destroy, desactivamos para devolverlo al Pool
            gameObject.SetActive(false);
        }
    }
}
