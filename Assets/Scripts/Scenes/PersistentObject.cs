using UnityEngine;

/// <summary>
/// Asegura que este GameObject persista entre escenas y que solo exista una instancia
/// de los sistemas persistentes, usando una bandera est�tica para evitar FindObjectOfType.
/// </summary>
public class PersistentObject : MonoBehaviour
{
    // Bandera est�tica para rastrear si los sistemas ya fueron inicializados.
    // Es "public get" para que otros scripts puedan leerla, pero "private set"
    // para que solo este script pueda modificarla.
    public static bool SystemsInitialized { get; private set; } = false;

    void Awake()
    {
        // Si los sistemas ya fueron inicializados por otra instancia...
        if (SystemsInitialized)
        {
            // ...entonces esta es una copia duplicada. Destr�yela y det�n la ejecuci�n.
            Debug.LogWarning("[PersistentObject] Se detect� un objeto de sistemas duplicado. Destruyendo...");
            Destroy(gameObject);
            return;
        }

        // Si llegamos aqu�, esta es la primera y �nica instancia.
        // Marca los sistemas como inicializados.
        SystemsInitialized = true;
        // Y haz que este objeto persista entre escenas.
        DontDestroyOnLoad(gameObject);
        Debug.Log("[PersistentObject] Sistemas persistentes inicializados.");
    }
}