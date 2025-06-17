using UnityEngine;

/// <summary>
/// ScriptableObject que representa una escena en el proyecto.
/// Usamos la ruta de la escena para evitar problemas con los build settings.
/// </summary>
[CreateAssetMenu(fileName = "NewSceneDefinition", menuName = "Scene Management/Scene Definition")]
public class SceneDefinitionSO : ScriptableObject
{
    [Tooltip("La ruta de la escena. ¡No editar manualmente! Usa el campo de SceneAsset de abajo.")]
    public string scenePath;

#if UNITY_EDITOR
    // Este campo solo existe en el editor para asignar la escena de forma segura.
    [SerializeField] private UnityEditor.SceneAsset sceneAsset;

    private void OnValidate()
    {
        if (sceneAsset != null)
        {
            scenePath = UnityEditor.AssetDatabase.GetAssetPath(sceneAsset);
        }
        else
        {
            scenePath = string.Empty;
        }
    }
#endif
}