using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class LevelButtonController : MonoBehaviour
{
    [Header("Configuraci�n del Nivel")]
    [Tooltip("La definici�n de la escena que este bot�n debe cargar.")]
    [SerializeField] private SceneDefinitionSO sceneToLoad;

    [Header("Canales de Eventos")]
    [SerializeField] private SceneLoadEventChannelSO sceneLoadChannel;
    [SerializeField] private SceneChannelSO activatePreloadedSceneChannel;

    [Header("Animaci�n")]
    [Tooltip("El componente gr�fico que parpadear�. Si se deja vac�o, buscar� una Imagen en este objeto.")]
    [SerializeField] private CanvasGroup buttonCanvasGroup;
    [SerializeField] private float blinkFadeValue = 0.5f;
    [SerializeField] private float blinkDuration = 0.4f;

    private bool _isSelected = false;

    // --- NUEVA L�NEA ---
    // Variable para guardar la referencia a nuestra animaci�n.
    private Tween _blinkTween;

    private void Awake()
    {
        if (buttonCanvasGroup == null)
        {
            buttonCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnLevelSelected()
    {
        if (_isSelected)
        {
            return;
        }
        _isSelected = true;
        StartCoroutine(LoadLevelSequence());
    }

    private System.Collections.IEnumerator LoadLevelSequence()
    {
        // --- MEJORA RECOMENDADA ---
        // Reanudamos el tiempo para asegurar que la siguiente escena no se cargue pausada.
        Time.timeScale = 1f;
        // --- L�NEA MODIFICADA ---
        // Guardamos la animaci�n en nuestra variable _blinkTween.
        _blinkTween = buttonCanvasGroup.DOFade(blinkFadeValue, blinkDuration)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);

        // Pide la PRE-CARGA
        sceneLoadChannel.RaiseEvent(sceneToLoad, UnityEngine.SceneManagement.LoadSceneMode.Single, true);

        // Usamos WaitForSecondsRealtime para que funcione incluso si Time.timeScale es 0.
        yield return new WaitForSecondsRealtime(0.1f);

        // Ahora esta l�nea S� se ejecutar�
        activatePreloadedSceneChannel.RaiseEvent();
    }

    // --- NUEVO M�TODO ---
    // Unity llama a este m�todo autom�ticamente cuando el objeto est� a punto de ser destruido.
    private void OnDestroy()
    {
        // Si nuestra animaci�n de parpadeo existe, la detenemos de forma segura.
        // Esto previene que DOTween intente acceder a un objeto destruido.
        if (_blinkTween != null)
        {
            _blinkTween.Kill();
        }
    }
}