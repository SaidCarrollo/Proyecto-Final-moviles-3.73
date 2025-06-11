
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Menus & Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsMenuPanel;
    [Header("Botones de UI")]
    public Button forceDespawnButton;

    void Start()
    {
        if (forceDespawnButton != null)
        {
            if (ProjectileManager.Instance != null)
            {
                forceDespawnButton.onClick.AddListener(ProjectileManager.Instance.ForceDespawnAllProjectiles);
            }
            else
            {
                Debug.LogError("No se encontr� una instancia de ProjectileManager. El bot�n de despawn no funcionar�.");
                forceDespawnButton.gameObject.SetActive(false);
            }
        }
    }
    public void TogglePauseMenu()
    {
        bool isActive = !pauseMenuPanel.activeSelf;
        pauseMenuPanel.SetActive(isActive);

        // Opcional: Pausar el juego cuando el men� se activa
        Time.timeScale = isActive ? 0f : 1f;
    }

    public void ShowSettingsMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(true);
        }
    }

    public void HideSettingsMenu()
    {
        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(false);
        }
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }

    // Un m�todo m�s gen�rico si lo prefieres
    public void SetGameObjectActive(GameObject targetObject)
    {
        targetObject.SetActive(true);
    }

    public void SetGameObjectInactive(GameObject targetObject)
    {
        targetObject.SetActive(false);
    }
}