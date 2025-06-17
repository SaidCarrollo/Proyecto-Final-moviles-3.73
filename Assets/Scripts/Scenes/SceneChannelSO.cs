using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/SceneChannelSO")]
public class SceneChannelSO : ScriptableObject
{
    public UnityAction OnEventRaised;

    public void RaiseEvent()
    {
        OnEventRaised?.Invoke();
    }
}