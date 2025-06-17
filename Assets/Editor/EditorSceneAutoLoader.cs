#if UNITY_EDITOR // Asegura que este script solo se compile y ejecute en el Editor de Unity
using UnityEngine;
using UnityEditor;

public class EditorSceneAutoLoader
{
    // La ruta de nuestro prefab DENTRO de una carpeta "Resources"
    private const string SYSTEMS_PREFAB_PATH = "SceneLoaderManager";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeSystems()
    {
        // En lugar de FindObjectOfType, ahora solo consultamos nuestra bandera estática.
        // ¡Esto es mucho más rápido y limpio!
        if (!PersistentObject.SystemsInitialized)
        {
            Debug.Log("[EditorSceneAutoLoader] No se encontraron sistemas. Cargando prefab...");

            var prefab = Resources.Load(SYSTEMS_PREFAB_PATH);

            if (prefab == null)
            {
                Debug.LogError($"[EditorSceneAutoLoader] No se encontró el prefab en 'Resources/{SYSTEMS_PREFAB_PATH}'. Asegúrate de que existe.");
                return;
            }

            Object.Instantiate(prefab);
            // El propio prefab, a través de su script PersistentObject, se encargará
            // de marcar la bandera SystemsInitialized como 'true'.
        }
    }
}
#endif