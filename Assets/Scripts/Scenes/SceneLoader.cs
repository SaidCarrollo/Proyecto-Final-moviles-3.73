using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening; // No olvides importar DOTween

public class SceneLoader : MonoBehaviour
{
    [Header("Canales de Eventos")]
    [SerializeField] private SceneLoadEventChannelSO sceneLoadChannel;
    [SerializeField] private SceneChannelSO activatePreloadedSceneChannel; // Canal para activar

    [Header("Transici�n")]
    [Tooltip("El CanvasGroup del panel negro que har� el fade")]
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
        // Suscribirse al evento de Unity para saber cu�ndo una escena termina de cargar
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

    // --- L�gica Principal ---

    private void HandleSceneLoadRequest(SceneDefinitionSO sceneToLoad, LoadSceneMode loadMode, bool isAsync)
    {
        // Si es as�ncrono, lo tratamos como una pre-carga
        if (isAsync)
        {
            if (_isPreloading)
            {
                Debug.LogWarning("Ya hay una escena pre-cargando.");
                return;
            }
            StartCoroutine(PreloadScene(sceneToLoad.scenePath, loadMode));
        }
        else // La carga s�ncrona sigue funcionando igual (bloquea y carga)
        {
            SceneManager.LoadScene(sceneToLoad.scenePath, loadMode);
        }
    }

    private IEnumerator PreloadScene(string scenePath, LoadSceneMode loadMode)
    {
        _isPreloading = true;
        _preloadedSceneOperation = SceneManager.LoadSceneAsync(scenePath, loadMode);
        _preloadedSceneOperation.allowSceneActivation = false; // �La clave!

        // Esperar a que la carga llegue al 90% (el punto antes de la activaci�n)
        while (_preloadedSceneOperation.progress < 0.9f)
        {
            yield return null;
        }

        Debug.Log($"Escena {scenePath} pre-cargada y lista para activar.");
        // La corrutina termina, pero la operaci�n est� viva y esperando.
    }

    private void ActivatePreloadedScene()
    {
        if (_preloadedSceneOperation == null || _isActivating)
        {
            Debug.LogWarning("No hay escena pre-cargada o ya se est� activando.");
            return;
        }

        _isActivating = true;
        // Iniciar transici�n de salida (fade out)
        transitionCanvasGroup.DOFade(1f, transitionDuration).OnComplete(() =>
        {
            // Cuando la pantalla est� en negro, permitir la activaci�n
            _preloadedSceneOperation.allowSceneActivation = true;
        });
    }

    // Se llama autom�ticamente cuando cualquier escena termina de cargar
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reseteamos estados y hacemos la transici�n de entrada (fade in)
        _isPreloading = false;
        _isActivating = false;
        transitionCanvasGroup.DOFade(0f, transitionDuration);
    }
}
