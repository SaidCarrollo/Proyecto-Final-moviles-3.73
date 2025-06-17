using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Canal de eventos para solicitar la carga de escenas.
/// </summary>
[CreateAssetMenu(fileName = "SceneLoadEventChannel", menuName = "Scene Management/Scene Load Event Channel")]
public class SceneLoadEventChannelSO : ScriptableObject
{
    // Definimos un tipo de evento que lleva los par�metros de la carga.
    // Par�metros: SceneDefinitionSO, modo de carga, si es as�ncrono.
    public UnityAction<SceneDefinitionSO, LoadSceneMode, bool> OnSceneRequested;

    /// <summary>
    /// M�todo para que los emisores levanten el evento de carga de escena.
    /// </summary>
    /// <param name="sceneToLoad">La definici�n de la escena a cargar.</param>
    /// <param name="loadMode">Single (reemplaza todo) o Additive (a�ade encima).</param>
    /// <param name="isAsync">True para carga as�ncrona (con pantalla de carga), false para carga s�ncrona (congela el juego).</param>
    public void RaiseEvent(SceneDefinitionSO sceneToLoad, LoadSceneMode loadMode = LoadSceneMode.Single, bool isAsync = true)
    {
        if (OnSceneRequested != null)
        {
            OnSceneRequested.Invoke(sceneToLoad, loadMode, isAsync);
        }
        else
        {
            Debug.LogWarning("Se solicit� una carga de escena, pero no hay ning�n receptor escuchando.");
        }
    }
}