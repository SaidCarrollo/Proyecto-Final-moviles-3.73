using UnityEngine;

public class ProgressManager : SingletonPersistent<ProgressManager>
{
    [Header("Event Channels")]
    [SerializeField] private LevelCompletedEventChannelSO onLevelCompleted;
    [SerializeField] private ProgressEventChannelSO onProgressUpdated;

    // Claves para guardar los datos localmente
    private const string LocalProgressKey = "HighestLevelUnlocked_Local";

    // El progreso actual del jugador (el �ndice del nivel m�s alto desbloqueado).
    // -1 significa que no ha desbloqueado ni el nivel 0.
    public int CurrentHighestLevel { get; private set; } = -1;

    public override void Awake()
    {
        base.Awake(); // Llama a la l�gica del Singleton
        LoadLocalProgress(); // Carga el progreso local al iniciar
    }

    private void OnEnable()
    {
        if (onLevelCompleted != null)
        {
            onLevelCompleted.OnEventRaised += HandleLevelCompleted;
        }
    }

    private void OnDisable()
    {
        if (onLevelCompleted != null)
        {
            onLevelCompleted.OnEventRaised -= HandleLevelCompleted;
        }
    }

    /// <summary>
    /// Se llama cuando el evento de nivel completado es disparado.
    /// </summary>
    private void HandleLevelCompleted(int justCompletedLevelIndex)
    {
        // Esta es la l�gica clave: solo desbloqueamos el siguiente nivel
        // si el nivel que acabamos de pasar es el M�S ALTO que hab�amos alcanzado.
        if (justCompletedLevelIndex == CurrentHighestLevel)
        {
            CurrentHighestLevel++;
            Debug.Log($"�Nuevo nivel desbloqueado! Ahora el m�s alto es: {CurrentHighestLevel}");
            SaveProgress(); // Guardamos el nuevo progreso
        }
        else
        {
            Debug.Log($"Nivel {justCompletedLevelIndex} completado de nuevo. No se desbloquean nuevos niveles.");
        }
    }

    /// <summary>
    /// Carga el progreso guardado en el dispositivo.
    /// </summary>
    public void LoadLocalProgress()
    {
        CurrentHighestLevel = PlayerPrefs.GetInt(LocalProgressKey, 0);
        Debug.Log($"Progreso local cargado. Nivel m�s alto: {CurrentHighestLevel}");
        onProgressUpdated?.RaiseEvent();
    }

    /// <summary>
    /// Guarda el progreso actual tanto localmente como en la nube (si aplica).
    /// </summary>
    private void SaveProgress()
    {
        // Guardado local
        PlayerPrefs.SetInt(LocalProgressKey, CurrentHighestLevel);
        PlayerPrefs.Save();
        Debug.Log($"Progreso guardado localmente: {CurrentHighestLevel}");

        // Guardado online
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.CurrentUser != null)
        {
            FirebaseManager.Instance.SaveProgressToFirestore(CurrentHighestLevel);
        }
    }

    /// <summary>
    /// Compara el progreso online con el local y decide cu�l mantener.
    /// </summary>
    public void SyncWithOnlineData(int onlineHighestLevel)
    {
        Debug.Log($"Sincronizando... Online: {onlineHighestLevel} vs Local: {CurrentHighestLevel}");
        if (onlineHighestLevel > CurrentHighestLevel)
        {
            // El progreso en la nube es mejor, lo adoptamos.
            CurrentHighestLevel = onlineHighestLevel;
            Debug.Log("El progreso online es superior. Usando datos de la nube.");
        }
        else if (CurrentHighestLevel > onlineHighestLevel)
        {
            // Nuestro progreso local es mejor, lo subimos a la nube.
            Debug.Log("El progreso local es superior. Subiendo a la nube.");
        }

        // Guardamos el estado final en ambos sitios para mantenerlos sincronizados.
        SaveProgress();
        onProgressUpdated?.RaiseEvent();
        // Podr�amos invocar aqu� un evento OnProgressUpdated para que la UI se refresque.
        // Por ahora, el LevelSelectorManager se actualiza en su OnEnable.
    }
    /// <summary>
    /// Borra el progreso guardado localmente desde PlayerPrefs (para testing).
    /// </summary>
    public void DeleteLocalProgress()
    {
        Debug.LogWarning($"Borrando progreso local guardado en la clave: {LocalProgressKey}");

        // Elimina la clave de PlayerPrefs
        PlayerPrefs.DeleteKey(LocalProgressKey);
        PlayerPrefs.Save(); // Es buena pr�ctica guardar inmediatamente despu�s de borrar.

        // Ahora, recarga el progreso local. Como la clave ya no existe,
        // se cargar� el valor por defecto (-1).
        LoadLocalProgress();

        // Finalmente, notifica a toda la UI que el estado ha cambiado para que se refresque.
        // Reutilizamos el mismo sistema de eventos que ya tenemos.
        if (FirebaseManager.Instance != null)
        {
            // Pedimos una actualizaci�n, lo que disparar� OnAuthStateChanged_Custom
            // y har� que el LevelSelectorManager se actualice.
            onProgressUpdated?.RaiseEvent();
        }
    }
}
