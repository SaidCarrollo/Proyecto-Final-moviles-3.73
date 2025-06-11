
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
                Debug.LogError("No se encontró una instancia de ProjectileManager. El botón de despawn no funcionará.");
                forceDespawnButton.gameObject.SetActive(false);
            }
        }
    }
    public void TogglePauseMenu()
    {
        bool isActive = !pauseMenuPanel.activeSelf;
        pauseMenuPanel.SetActive(isActive);

        // Opcional: Pausar el juego cuando el menú se activa
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

    // Un método más genérico si lo prefieres
    public void SetGameObjectActive(GameObject targetObject)
    {
        targetObject.SetActive(true);
    }

    public void SetGameObjectInactive(GameObject targetObject)
    {
        targetObject.SetActive(false);
    }
}