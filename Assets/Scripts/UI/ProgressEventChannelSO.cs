using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Progress Event Channel")]
public class ProgressEventChannelSO : ScriptableObject
{
    public UnityAction OnEventRaised;

    public void RaiseEvent()
    {
        OnEventRaised?.Invoke();
    }
}