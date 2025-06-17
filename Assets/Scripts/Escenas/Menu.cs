using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SceneData", menuName = "Game/Scene Data")]
public class SceneData : ScriptableObject
{
    public string mainMenu;
    public string login;
    public string register;
    public string niveles;
    public string[] levels; 
    public string results;
    public string gameOver;
    public string gameWin;
}
public class Menu : MonoBehaviour
{

    [SerializeField] private SceneData sceneData;
    [SerializeField] private GameObject pausePanel;
    public void LoadScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
      
    }
    public void Pause()
    {
        bool isPaused = !pausePanel.activeSelf;
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1; 
    }
    public void LoadMainMenu() => LoadScene(sceneData.mainMenu);
    public void LoadLevel(int index) => LoadScene(sceneData.levels[index]);
    public void RestartLevel() => LoadScene(SceneManager.GetActiveScene().name);


    public void ExitGame()
    {
        Application.Quit();

    }
}
