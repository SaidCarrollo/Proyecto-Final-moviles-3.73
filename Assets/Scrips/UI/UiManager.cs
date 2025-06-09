
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
                Debug.LogError("No se encontr� una instancia de ProjectileManager. El bot�n de despawn no funcionar�.");
                forceDespawnButton.gameObject.SetActive(false);
            }
        }
    }
}