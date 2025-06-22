using UnityEngine;

public class LevelCompletionTrigger : MonoBehaviour
{
    [Header("Event Channel")]
    [Tooltip("El canal que se usará para notificar que el nivel se completó.")]
    [SerializeField] private LevelCompletedEventChannelSO onLevelCompleted;

    [Header("Level Info")]
    [Tooltip("El índice de ESTE nivel (ej: Nivel 1 es índice 0, Nivel 2 es 1, etc.).")]
    [SerializeField] private int thisLevelIndex;

    // Puedes llamar a este método desde un botón de "Continuar" o un trigger al final del nivel.
    public void CompleteLevel()
    {
        Debug.Log($"Disparando evento de Nivel completado para el índice: {thisLevelIndex}");
        onLevelCompleted.RaiseEvent(thisLevelIndex);
    }
}