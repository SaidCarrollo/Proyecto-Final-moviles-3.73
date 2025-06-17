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
        // En lugar de FindObjectOfType, ahora solo consultamos nuestra bandera est�tica.
        // �Esto es mucho m�s r�pido y limpio!
        if (!PersistentObject.SystemsInitialized)
        {
            Debug.Log("[EditorSceneAutoLoader] No se encontraron sistemas. Cargando prefab...");

            var prefab = Resources.Load(SYSTEMS_PREFAB_PATH);

            if (prefab == null)
            {
                Debug.LogError($"[EditorSceneAutoLoader] No se encontr� el prefab en 'Resources/{SYSTEMS_PREFAB_PATH}'. Aseg�rate de que existe.");
                return;
            }

            Object.Instantiate(prefab);
            // El propio prefab, a trav�s de su script PersistentObject, se encargar�
            // de marcar la bandera SystemsInitialized como 'true'.
        }
    }
}
#endif