using UnityEngine;

public class PotenciadorEscudo : MonoBehaviour
{
    // Ya no necesitamos la variable duracionEscudo aquí, 
    // porque ahora la decides tú desde el ControladorEscudo del jugador.

    private void OnTriggerEnter(Collider other)
    {
        // Buscamos el componente en el objeto que chocó o en sus padres
        ControladorEscudo controlador = other.GetComponentInParent<ControladorEscudo>();

        if (controlador != null)
        {
            // Llamamos al nuevo método que no pide parámetros
            controlador.ActivarEscudoDesdeItem();
            
            // Desactivamos el ítem para que el GestorPools lo pueda reutilizar
            gameObject.SetActive(false); 
        }
    }
}