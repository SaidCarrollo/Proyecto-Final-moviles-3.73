
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
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
}