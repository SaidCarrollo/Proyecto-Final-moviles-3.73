using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening; // No olvides importar DOTween

public class SceneLoader : MonoBehaviour
{
    [Header("Canales de Eventos")]
    [SerializeField] private SceneLoadEventChannelSO sceneLoadChannel;
    [SerializeField] private SceneChannelSO activatePreloadedSceneChannel; // Canal para activar

    [Header("Transición")]
    [Tooltip("El CanvasGroup del panel negro que hará el fade")]
    [SerializeField] private CanvasGroup transitionCanvasGroup;
    [SerializeField] private float transitionDuration = 0.5f;

    // Variables para manejar la pre-carga
    private AsyncOperation _preloadedSceneOperation;
    private bool _isPreloading = false;
    private bool _isActivating = false;

    private void OnEnable()
    {
        if (sceneLoadChannel != null)
        {
            sceneLoadChannel.OnSceneRequested += HandleSceneLoadRequest;
        }
        if (activatePreloadedSceneChannel != null)
        {
            activatePreloadedSceneChannel.OnEventRaised += ActivatePreloadedScene;
        }
        // Suscribirse al evento de Unity para saber cuándo una escena termina de cargar
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (sceneLoadChannel != null)
        {
            sceneLoadChannel.OnSceneRequested -= HandleSceneLoadRequest;
        }
        if (activatePreloadedSceneChannel != null)
        {
            activatePreloadedSceneChannel.OnEventRaised -= ActivatePreloadedScene;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- Lógica Principal ---

    private void HandleSceneLoadRequest(SceneDefinitionSO sceneToLoad, LoadSceneMode loadMode, bool isAsync)
    {
        // Si es asíncrono, lo tratamos como una pre-carga
        if (isAsync)
        {
            if (_isPreloading)
            {
                Debug.LogWarning("Ya hay una escena pre-cargando.");
                return;
            }
            StartCoroutine(PreloadScene(sceneToLoad.scenePath, loadMode));
        }
        else // La carga síncrona sigue funcionando igual (bloquea y carga)
        {
            SceneManager.LoadScene(sceneToLoad.scenePath, loadMode);
        }
    }

    private IEnumerator PreloadScene(string scenePath, LoadSceneMode loadMode)
    {
        _isPreloading = true;
        _preloadedSceneOperation = SceneManager.LoadSceneAsync(scenePath, loadMode);
        _preloadedSceneOperation.allowSceneActivation = false; // ¡La clave!

        // Esperar a que la carga llegue al 90% (el punto antes de la activación)
        while (_preloadedSceneOperation.progress < 0.9f)
        {
            yield return null;
        }

        Debug.Log($"Escena {scenePath} pre-cargada y lista para activar.");
        // La corrutina termina, pero la operación está viva y esperando.
    }

    private void ActivatePreloadedScene()
    {
        if (_preloadedSceneOperation == null || _isActivating)
        {
            Debug.LogWarning("No hay escena pre-cargada o ya se está activando.");
            return;
        }

        _isActivating = true;
        // Iniciar transición de salida (fade out)
        transitionCanvasGroup.DOFade(1f, transitionDuration).OnComplete(() =>
        {
            // Cuando la pantalla está en negro, permitir la activación
            _preloadedSceneOperation.allowSceneActivation = true;
        });
    }

    // Se llama automáticamente cuando cualquier escena termina de cargar
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reseteamos estados y hacemos la transición de entrada (fade in)
        _isPreloading = false;
        _isActivating = false;
        transitionCanvasGroup.DOFade(0f, transitionDuration);
    }
}
