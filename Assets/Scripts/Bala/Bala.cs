using UnityEngine;
using System.Collections;

public class Bala : MonoBehaviour
{
    public int dano = 1;
    public float tiempoDeVida = 3f;

    private void OnEnable()
    {
        StartCoroutine(RutinaDevolverDespuesDeTiempo());
    }

    private IEnumerator RutinaDevolverDespuesDeTiempo()
    {
        yield return new WaitForSeconds(tiempoDeVida);
        GestorPools.Instancia.DevolverAGrupo("ProyectilBala", gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        StopAllCoroutines();

        if (collision.gameObject.CompareTag("Zombie"))
        {
            ControladorZombi zombi = collision.gameObject.GetComponent<ControladorZombi>();
            if (zombi != null)
            {
                zombi.RecibirDano(dano);
            }
        }
        
        GestorPools.Instancia.DevolverAGrupo("ProyectilBala", gameObject);
    }
}
