using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        SceneLoader.Instance.LoadScene("Menu");
    }
}
