using UnityEngine;

public class Bala : MonoBehaviour
{
    public int dano = 10;

    void Start()
    {
        Destroy(gameObject, 3f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Zombie"))
        {
            collision.gameObject.GetComponent<SaludZombi>().RecibirDano(dano);
        }
        Destroy(gameObject);
    }
}