using UnityEngine;

/// <summary>
/// Asegura que este GameObject persista entre escenas y que solo exista una instancia
/// de los sistemas persistentes, usando una bandera estática para evitar FindObjectOfType.
/// </summary>
public class PersistentObject : MonoBehaviour
{
    // Bandera estática para rastrear si los sistemas ya fueron inicializados.
    // Es "public get" para que otros scripts puedan leerla, pero "private set"
    // para que solo este script pueda modificarla.
    public static bool SystemsInitialized { get; private set; } = false;

    void Awake()
    {
        // Si los sistemas ya fueron inicializados por otra instancia...
        if (SystemsInitialized)
        {
            // ...entonces esta es una copia duplicada. Destrúyela y detén la ejecución.
            Debug.LogWarning("[PersistentObject] Se detectó un objeto de sistemas duplicado. Destruyendo...");
            Destroy(gameObject);
            return;
        }

        // Si llegamos aquí, esta es la primera y única instancia.
        // Marca los sistemas como inicializados.
        SystemsInitialized = true;
        // Y haz que este objeto persista entre escenas.
        DontDestroyOnLoad(gameObject);
        Debug.Log("[PersistentObject] Sistemas persistentes inicializados.");
    }
}