using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string loginScene = "IniciaSesion";
    [SerializeField] private string registerScene = "Registro";
    [SerializeField] private string nivelesScene = "Niveles";
    [SerializeField] private string level1Scene = "Level1";
    [SerializeField] private string level2Scene = "Level2";
    [SerializeField] private string level3Scene = "Level3";
    [SerializeField] private string level4Scene = "Level4";
    [SerializeField] private string level5Scene = "Level5";
    [SerializeField] private string pauseMenuScene = "PauseMenu";
    [SerializeField] private string resultsScene = "Results";
    [SerializeField] private string gameOverScene = "GameOver";
    [SerializeField] private string gameWinScene = "GameWin";

    public void LoadMainMenu()
    {
        LoadLevel(mainMenuScene);
    }

    public void LoadLogin()
    {
        LoadLevel(loginScene);
    }

    public void LoadRegister()
    {
        LoadLevel(registerScene);
    }

    public void LoadNiveles()
    {
        LoadLevel(nivelesScene);
    }

    public void StartGame()
    {
        LoadLevel(level1Scene);
    }

    public void LoadLevel2() => LoadLevel(level2Scene);
    public void LoadLevel3() => LoadLevel(level3Scene);
    public void LoadLevel4() => LoadLevel(level4Scene);
    public void LoadLevel5() => LoadLevel(level5Scene);

    public void LoadPauseMenu()
    {
        LoadLevel(pauseMenuScene);
    }

    public void ShowResults()
    {
        LoadLevel(resultsScene);
    }

    public void GameOver()
    {
        LoadLevel(gameOverScene);
    }

    public void WinGame()
    {
        LoadLevel(gameWinScene);
    }

    public void RestartCurrentLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadLevel(currentScene);
    }

    public void LoadNextLevel()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log(" No hay más niveles. ¡Ganaste el juego!");
            LoadLevel(gameWinScene);
        }
    }

    private void LoadLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError(" Nombre de escena vacío o nulo.");
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($" La escena '{sceneName}' no está en Build Settings o está mal escrita.");
        }
    }
}
