using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SceneLoader : MonoBehaviour
{
    [Header("Configuración de Carga")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private float minLoadDuration = 1.5f;
    [SerializeField] private bool debugLogs = true;

    public static SceneLoader Instance { get; private set; }

    private List<string> loadedAdditiveScenes = new List<string>();
    private Coroutine currentLoadingRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
    }

    public static void LoadSceneStatic(string sceneName, LoadMode mode = LoadMode.Single, bool showLoadingScreen = true)
    {
        if (Instance != null)
        {
            Instance.LoadSceneInstance(sceneName, mode, showLoadingScreen);
        }
        else
        {
            Debug.LogError("SceneLoader no está inicializado");
            // Fallback: carga directa
            SceneManager.LoadScene(sceneName);
        }
    }

    // Método de instancia original (renombrado para claridad)
    public void LoadSceneInstance(string sceneName, LoadMode mode = LoadMode.Single, bool showLoadingScreen = true)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Nombre de escena no válido");
            return;
        }

        if (currentLoadingRoutine != null)
        {
            StopCoroutine(currentLoadingRoutine);
        }

        currentLoadingRoutine = StartCoroutine(LoadSceneRoutine(sceneName, mode, showLoadingScreen));
    }

    // ==== MÉTODOS PÚBLICOS PRINCIPALES ==== //

    public void LoadScene(string sceneName, LoadMode mode = LoadMode.Single, bool showLoadingScreen = true)
    {
        if (currentLoadingRoutine != null)
        {
            StopCoroutine(currentLoadingRoutine);
        }

        currentLoadingRoutine = StartCoroutine(LoadSceneRoutine(sceneName, mode, showLoadingScreen));
    }

    public void UnloadScene(string sceneName)
    {
        if (IsSceneLoaded(sceneName))
        {
            StartCoroutine(UnloadSceneRoutine(sceneName));
        }
        else if (debugLogs)
        {
            Debug.LogWarning($"Escena '{sceneName}' no está cargada para descargar.");
        }
    }

    public void SwitchActiveScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded)
        {
            SceneManager.SetActiveScene(scene);
        }
    }

    public bool IsSceneLoaded(string sceneName)
    {
        return loadedAdditiveScenes.Contains(sceneName);
    }

    // ==== CORRUTINAS DE CARGA ==== //

    private IEnumerator LoadSceneRoutine(string sceneName, LoadMode mode, bool showLoading)
    {
        if (debugLogs) Debug.Log($"Iniciando carga de '{sceneName}' en modo {mode}");

        // Activar pantalla de carga si es necesario
        if (showLoading && loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }

        float startTime = Time.time;

        // Limpiar escenas si es modo Single
        if (mode == LoadMode.Single)
        {
            yield return CleanupAdditiveScenes();
        }

        // Cargar la escena principal
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName,
            mode == LoadMode.Single ? LoadSceneMode.Single : LoadSceneMode.Additive);

        loadOperation.allowSceneActivation = false;

        // Simular carga mínima
        while (Time.time - startTime < minLoadDuration || loadOperation.progress < 0.9f)
        {
            yield return null;
        }

        loadOperation.allowSceneActivation = true;

        // Esperar a que termine completamente
        while (!loadOperation.isDone)
        {
            yield return null;
        }

        // Desactivar pantalla de carga
        if (showLoading && loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }

        if (debugLogs) Debug.Log($"Carga de '{sceneName}' completada");
    }

    private IEnumerator UnloadSceneRoutine(string sceneName)
    {
        if (debugLogs) Debug.Log($"Iniciando descarga de '{sceneName}'");

        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(sceneName);

        while (!unloadOperation.isDone)
        {
            yield return null;
        }

        Resources.UnloadUnusedAssets();

        if (debugLogs) Debug.Log($"Descarga de '{sceneName}' completada");
    }

    private IEnumerator CleanupAdditiveScenes()
    {
        if (loadedAdditiveScenes.Count == 0) yield break;

        if (debugLogs) Debug.Log("Limpiando escenas aditivas...");

        List<string> scenesToUnload = new List<string>(loadedAdditiveScenes);

        foreach (string sceneName in scenesToUnload)
        {
            yield return UnloadSceneRoutine(sceneName);
        }

        loadedAdditiveScenes.Clear();
    }

    // ==== MANEJADORES DE EVENTOS ==== //

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive && !loadedAdditiveScenes.Contains(scene.name))
        {
            loadedAdditiveScenes.Add(scene.name);
            if (debugLogs) Debug.Log($"Escena aditiva '{scene.name}' registrada");
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (loadedAdditiveScenes.Contains(scene.name))
        {
            loadedAdditiveScenes.Remove(scene.name);
            if (debugLogs) Debug.Log($"Escena aditiva '{scene.name}' eliminada del registro");
        }
    }
}

public enum LoadMode
{
    Single,     
    Additive    
}
