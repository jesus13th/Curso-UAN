using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame() {//creamos un metodo publico para iniciar el juego o cambiar de escena
        SceneManager.LoadScene("Game");//cambiamos a la escena llamada "Game"
    }
    public void QuitGame() {//metodo para quitar el juego
        Application.Quit();//quitamos el juego con el metodo Quit de la clase Application
    }
}
