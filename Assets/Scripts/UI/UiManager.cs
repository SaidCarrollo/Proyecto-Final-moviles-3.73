using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Menus & Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsMenuPanel;
    [Tooltip("Arrastra aquí el panel que se muestra al ganar.")]
    [SerializeField] private GameObject winPanel;
    [Tooltip("Arrastra aquí el panel que se muestra al perder.")]
    [SerializeField] private GameObject losePanel;

    [Header("Botones de UI")]
    public Button forceDespawnButton;
    [Tooltip("Botón para centrar la cámara en la resortera.")]
    public Button focusSlingshotButton;

    private CameraFollowProjectile cameraFollower;

    private void OnEnable()
    {
        GameManager.OnGameWon += ShowWinPanel;
        GameManager.OnGameLost += ShowLosePanel;
    }

    private void OnDisable()
    {
        GameManager.OnGameWon -= ShowWinPanel;
        GameManager.OnGameLost -= ShowLosePanel;
    }

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

        if (Camera.main != null)
        {
            cameraFollower = Camera.main.GetComponent<CameraFollowProjectile>();
        }

        if (focusSlingshotButton != null)
        {
            if (cameraFollower != null)
            {
                focusSlingshotButton.onClick.AddListener(OnFocusSlingshotButtonPressed);
                focusSlingshotButton.gameObject.SetActive(true); 
            }
            else
            {
                Debug.LogError("No se encontró el script CameraFollowProjectile en la cámara principal. El botón de enfoque no funcionará.");
                focusSlingshotButton.gameObject.SetActive(false);
            }
        }
    }

    public void OnFocusSlingshotButtonPressed()
    {
        if (cameraFollower != null)
        {
            cameraFollower.ToggleCameraView();
        }
    }


    private void ShowWinPanel()
    {
        if (winPanel != null)
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            winPanel.SetActive(true);
        }
    }

    private void ShowLosePanel()
    {
        if (losePanel != null)
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            losePanel.SetActive(true);
        }
    }

    public void TogglePauseMenu()
    {
        bool isActive = !pauseMenuPanel.activeSelf;
        pauseMenuPanel.SetActive(isActive);

        if (winPanel.activeSelf || losePanel.activeSelf)
        {
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = isActive ? 0f : 1f;
        }
    }

    public void ShowSettingsMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(true);
    }

    public void HideSettingsMenu()
    {
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    public void SetGameObjectActive(GameObject targetObject)
    {
        targetObject.SetActive(true);
    }

    public void SetGameObjectInactive(GameObject targetObject)
    {
        targetObject.SetActive(false);
    }
}