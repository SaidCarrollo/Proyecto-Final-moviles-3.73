using UnityEngine;

public class DeleteProgressButton : MonoBehaviour
{
    // Este método se conectará al evento OnClick del botón en el Inspector.
    public void HandleDeleteClick()
    {
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.DeleteLocalProgress();
        }
        else
        {
            Debug.LogError("ProgressManager.Instance no encontrado. Asegúrate de que los sistemas estén inicializados.");
        }
    }
}