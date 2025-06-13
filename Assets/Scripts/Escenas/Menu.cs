using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    private string currentLevel;

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadSceneInstance(mainMenuScene, LoadMode.Single);
            
        }
        else
        {
            // Fallback
            SceneManager.LoadScene(mainMenuScene);
        }
    }

    public void LoadLogin()
    {
        Time.timeScale = 1f;
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(loginScene, LoadMode.Single);
        }
    }

    public void LoadRegister()
    {
        Time.timeScale = 1f;
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(registerScene, LoadMode.Single);
        }
    }

    public void LoadNiveles()
    {
        Time.timeScale = 1f;
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(nivelesScene, LoadMode.Single);
        }
    }

    public void StartGame()
    {
        currentLevel = level1Scene;
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(currentLevel, LoadMode.Single);
            SceneLoader.Instance.LoadScene(resultsScene, LoadMode.Additive, false);
        }
    }

    public void OpenPauseMenu()
    {
        //if (SceneLoader.Instance != null)
        //{
            SceneLoader.Instance.LoadScene(pauseMenuScene, LoadMode.Additive);
            Time.timeScale = 0f;
        //}
    }

    public void ClosePauseMenu()
    {
        //if (SceneLoader.Instance != null)
        //{
            SceneLoader.Instance.UnloadScene(pauseMenuScene);
            Time.timeScale = 1f;
        //}
    }

    public void ShowResults()
    {
        //if (SceneLoader.Instance.IsSceneLoaded(resultsScene))
        //{
            Scene resultsSceneObj = SceneManager.GetSceneByName(resultsScene);
            foreach (GameObject obj in resultsSceneObj.GetRootGameObjects())
            {
                obj.SetActive(true);
            }
        //}
    }

    public void LoadLevel(string levelName)
    {
        //if (SceneLoader.Instance != null)
        //{
            currentLevel = levelName;
            SceneLoader.Instance.LoadScene(currentLevel, LoadMode.Single);
            SceneLoader.Instance.LoadScene(resultsScene, LoadMode.Additive, false);
        //}
    }

    public void LoadNextLevel()
    {
        switch (currentLevel)
        {
            case "Level1":
                currentLevel = level2Scene;
                break;
            case "Level2":
                currentLevel = level3Scene;
                break;
            case "Level3":
                currentLevel = level4Scene;
                break;
            case "Level4":
                currentLevel = level5Scene;
                break;
            case "Level5":
                WinGame();
                return;
            default:
                currentLevel = level1Scene;
                break;
        }

        //if (SceneLoader.Instance != null)
        //{
            SceneLoader.Instance.LoadScene(currentLevel, LoadMode.Single);
            SceneLoader.Instance.LoadScene(resultsScene, LoadMode.Additive, false);
        //}
    }

    public void RestartCurrentLevel()
    {
        //if (SceneLoader.Instance != null)
        //{
            SceneLoader.Instance.LoadScene(currentLevel, LoadMode.Single);
            SceneLoader.Instance.LoadScene(resultsScene, LoadMode.Additive, false);
        //}
    }

    public void GameOver()
    {
        //if (SceneLoader.Instance != null)
        //{
            SceneLoader.Instance.LoadScene(gameOverScene, LoadMode.Single);
        //}
    }

    public void WinGame()
    {
        //if (SceneLoader.Instance != null)
        //{
           SceneLoader.Instance.LoadScene(gameWinScene, LoadMode.Single);
        //}
    }

    public void LoadLevel1() => LoadLevel(level1Scene);
    public void LoadLevel2() => LoadLevel(level2Scene);
    public void LoadLevel3() => LoadLevel(level3Scene);
    public void LoadLevel4() => LoadLevel(level4Scene);
    public void LoadLevel5() => LoadLevel(level5Scene);
}

