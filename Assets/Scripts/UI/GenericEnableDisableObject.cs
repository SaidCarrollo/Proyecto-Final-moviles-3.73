using UnityEngine;
using UnityEngine.UI;

public class GenericEnableDisableObject : MonoBehaviour
{
    public void SetGameObjectActive(GameObject targetObject)
    {
        targetObject.SetActive(true);
    }

    public void SetGameObjectInactive(GameObject targetObject)
    {
        targetObject.SetActive(false);
    }
}
