using UnityEngine;

public class LevelCompletionTrigger : MonoBehaviour
{
    [Header("Event Channel")]
    [Tooltip("El canal que se usar� para notificar que el nivel se complet�.")]
    [SerializeField] private LevelCompletedEventChannelSO onLevelCompleted;

    [Header("Level Info")]
    [Tooltip("El �ndice de ESTE nivel (ej: Nivel 1 es �ndice 0, Nivel 2 es 1, etc.).")]
    [SerializeField] private int thisLevelIndex;

    // Puedes llamar a este m�todo desde un bot�n de "Continuar" o un trigger al final del nivel.
    public void CompleteLevel()
    {
        Debug.Log($"Disparando evento de Nivel completado para el �ndice: {thisLevelIndex}");
        onLevelCompleted.RaiseEvent(thisLevelIndex);
    }
}