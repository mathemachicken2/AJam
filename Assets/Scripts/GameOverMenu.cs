using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenu";

    public void TryAgain()
    {
        Debug.Log("TRY AGAIN CLICKED");
        Scene currentScene = SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}