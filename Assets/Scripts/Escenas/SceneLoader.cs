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

            if (debugLogs) Debug.Log("SceneLoader inicializado correctamente");
        }
        else if (Instance != this)
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

    public void LoadScene(string sceneName, LoadMode mode = LoadMode.Single, bool showLoadingScreen = true)
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

    private IEnumerator LoadSceneRoutine(string sceneName, LoadMode mode, bool showLoading)
    {
        if (debugLogs) Debug.Log($"Iniciando carga de '{sceneName}' en modo {mode}");

        // Mostrar pantalla de carga
        if (showLoading && loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            if (debugLogs) Debug.Log("Pantalla de carga activada");
        }

        float startTime = Time.time;

        if (mode == LoadMode.Single)
        {
            yield return CleanupAdditiveScenes();
        }

        // Cargar la escena
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName,
            mode == LoadMode.Single ? LoadSceneMode.Single : LoadSceneMode.Additive);

        loadOperation.allowSceneActivation = false;

        // Esperar el tiempo mínimo de carga
        while (Time.time - startTime < minLoadDuration || loadOperation.progress < 0.9f)
        {
            yield return null;
        }

        // Permitir activación de la escena
        loadOperation.allowSceneActivation = true;

        // Esperar a que termine completamente
        while (!loadOperation.isDone)
        {
            yield return null;
        }

        // Ocultar pantalla de carga
        if (showLoading && loadingScreen != null)
        {
            loadingScreen.SetActive(false);
            if (debugLogs) Debug.Log("Pantalla de carga desactivada");
        }

        if (debugLogs) Debug.Log($"Escena '{sceneName}' cargada completamente");
    }

    public void UnloadScene(string sceneName)
    {
        if (IsSceneLoaded(sceneName))
        {
            StartCoroutine(UnloadSceneRoutine(sceneName));
        }
        else if (debugLogs)
        {
            Debug.LogWarning($"Escena '{sceneName}' no está cargada para descargar");
        }
    }

    private IEnumerator UnloadSceneRoutine(string sceneName)
    {
        if (debugLogs) Debug.Log($"Descargando escena '{sceneName}'");

        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(sceneName);

        while (!unloadOperation.isDone)
        {
            yield return null;
        }

        Resources.UnloadUnusedAssets();

        if (debugLogs) Debug.Log($"Escena '{sceneName}' descargada completamente");
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

    public bool IsSceneLoaded(string sceneName)
    {
        return loadedAdditiveScenes.Contains(sceneName);
    }

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