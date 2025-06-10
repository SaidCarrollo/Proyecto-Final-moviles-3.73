using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void LoadScene1()
    {
        SceneManager.LoadScene("Scene1"); 

    }

    public void LoadScene2()
    {
        SceneManager.LoadScene("Scene2");
    }

    public void LoadScene3()
    {
        SceneManager.LoadScene("Scene3");
    }

    public void Exit()
    {
        Debug.Log("Saliste");
        Application.Quit();
    }
}