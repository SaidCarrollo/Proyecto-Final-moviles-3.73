using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    // Referencia al prefab de sistemas (puedes arrastrarlo en el inspector)
    [SerializeField] private GameObject systemsPrefab;
    // Referencia a la escena del Menú Principal
    [SerializeField] private SceneDefinitionSO mainMenuScene;
    [SerializeField] private SceneLoadEventChannelSO sceneLoadChannel;

    void Start()
    {
        // Instancia los sistemas
        Instantiate(systemsPrefab);

        // Una vez instanciados, espera un frame para asegurar que se suscriban a eventos
        // y luego carga el menú principal de forma síncrona y sin transición.
        // Usamos el mismo sistema de eventos para mantener la consistencia.
        sceneLoadChannel.RaiseEvent(mainMenuScene, UnityEngine.SceneManagement.LoadSceneMode.Single, false);
    }
}