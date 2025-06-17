using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Inicia la pre-carga de una escena en segundo plano al iniciar.
/// </summary>
public class ScenePreloader : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private SceneDefinitionSO sceneToPreload;
    [SerializeField] private LoadSceneMode loadMode = LoadSceneMode.Single;

    [Header("Canal")]
    [SerializeField] private SceneLoadEventChannelSO sceneLoadChannel;

    void Start()
    {
        // En cuanto el objeto se active, pide la pre-carga de la escena.
        sceneLoadChannel.RaiseEvent(sceneToPreload, loadMode, true);
    }
}