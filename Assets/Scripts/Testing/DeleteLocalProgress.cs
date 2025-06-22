using UnityEngine;

public class DeleteProgressButton : MonoBehaviour
{
    // Este m�todo se conectar� al evento OnClick del bot�n en el Inspector.
    public void HandleDeleteClick()
    {
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.DeleteLocalProgress();
        }
        else
        {
            Debug.LogError("ProgressManager.Instance no encontrado. Aseg�rate de que los sistemas est�n inicializados.");
        }
    }
}