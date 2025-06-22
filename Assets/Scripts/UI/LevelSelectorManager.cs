using UnityEngine;
using UnityEngine.UI;

public class LevelSelectorManager : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private ProgressEventChannelSO onProgressUpdated;

    [Tooltip("Arrastra aquí los componentes 'Image' de los paneles que bloquean cada nivel.")]
    public Image[] levelBlockers;

    [Tooltip("Elige el color y la transparencia que tendrán los niveles bloqueados.")]
    public Color lockedColor = new Color(0f, 0f, 0f, 0.5f);

    void OnEnable()
    {
        // Nos seguimos suscribiendo al cambio de estado, ya que es nuestra señal para refrescar.
        if (onProgressUpdated != null) onProgressUpdated.OnEventRaised += UpdateLevelVisibility;

        // Forzamos una actualización inicial
        UpdateLevelVisibility();
    }

    void OnDisable()
    {
        if (onProgressUpdated != null) onProgressUpdated.OnEventRaised -= UpdateLevelVisibility;
    }

    /// <summary>
    /// Se llama cada vez que el estado de autenticación cambie.
    /// Su única misión es pedir el estado de progreso actual al ProgressManager.
    /// </summary>
    private void UpdateLevelVisibility()
    {
        if (ProgressManager.Instance != null)
        {
            int progress = ProgressManager.Instance.CurrentHighestLevel;
            UnlockLevels(progress);
        }
        else
        {
            UnlockLevels(-1);
        }
    }

    public void UnlockLevels(int highestLevelUnlocked)
    {
        // He notado que en tu código anterior, en el 'else' de HandleAuthStateChanged,
        // llamabas a UnlockLevels(0). Esto desbloquearía el nivel 0. 
        // El valor correcto para bloquear todo es -1.

        if (levelBlockers == null) return;

        Debug.Log($"Actualizando UI de niveles. Nivel más alto actual: {highestLevelUnlocked}");

        for (int i = 0; i < levelBlockers.Length; i++)
        {
            if (levelBlockers[i] == null) continue;

            Image blockerImage = levelBlockers[i];
            int levelToCheck = i;

            if (levelToCheck <= highestLevelUnlocked)
            {
                blockerImage.raycastTarget = false;
                blockerImage.color = Color.clear;
            }
            else
            {
                blockerImage.raycastTarget = true;
                blockerImage.color = lockedColor;
            }
        }
    }
}