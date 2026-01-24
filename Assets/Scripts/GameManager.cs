using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void EmpezarPartida()
    {
        SceneManager.LoadScene("Juego");
    }

    public void VolverAJugar()
    {
        SceneManager.LoadScene("Juego");
    }

    public void SiguienteNivel()
    {
        SceneManager.LoadScene("Juego");
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}