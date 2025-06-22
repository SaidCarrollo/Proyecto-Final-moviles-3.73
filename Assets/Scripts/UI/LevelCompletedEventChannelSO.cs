using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Level Completed Event Channel")]
public class LevelCompletedEventChannelSO : ScriptableObject
{
    public UnityAction<int> OnEventRaised;

    public void RaiseEvent(int levelIndex)
    {
        // Notifica a todos los que estén escuchando.
        OnEventRaised?.Invoke(levelIndex);
    }
}