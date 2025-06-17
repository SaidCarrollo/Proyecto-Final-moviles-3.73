using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Canal de eventos para solicitar la carga de escenas.
/// </summary>
[CreateAssetMenu(fileName = "SceneLoadEventChannel", menuName = "Scene Management/Scene Load Event Channel")]
public class SceneLoadEventChannelSO : ScriptableObject
{
    // Definimos un tipo de evento que lleva los parámetros de la carga.
    // Parámetros: SceneDefinitionSO, modo de carga, si es asíncrono.
    public UnityAction<SceneDefinitionSO, LoadSceneMode, bool> OnSceneRequested;

    /// <summary>
    /// Método para que los emisores levanten el evento de carga de escena.
    /// </summary>
    /// <param name="sceneToLoad">La definición de la escena a cargar.</param>
    /// <param name="loadMode">Single (reemplaza todo) o Additive (añade encima).</param>
    /// <param name="isAsync">True para carga asíncrona (con pantalla de carga), false para carga síncrona (congela el juego).</param>
    public void RaiseEvent(SceneDefinitionSO sceneToLoad, LoadSceneMode loadMode = LoadSceneMode.Single, bool isAsync = true)
    {
        if (OnSceneRequested != null)
        {
            OnSceneRequested.Invoke(sceneToLoad, loadMode, isAsync);
        }
        else
        {
            Debug.LogWarning("Se solicitó una carga de escena, pero no hay ningún receptor escuchando.");
        }
    }
}