using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AccountDeletionController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("El campo para que el usuario ingrese su contrase�a para confirmar.")]
    [SerializeField] private TMP_InputField passwordInputField;

    [Tooltip("El bot�n que inicia el proceso de eliminaci�n.")]
    [SerializeField] private Button deleteButton;

    // No necesitas un campo de texto para feedback aqu�, ya que reutilizaremos 
    // el sistema de feedback que ya tienes en LoginPanelController.

    /// <summary>
    /// Este m�todo debe ser llamado desde el evento OnClick del bot�n de eliminar.
    /// </summary>
    public void HandleDeleteAccountRequest()
    {
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(password))
        {
            // Enviamos el mensaje a trav�s del sistema de feedback centralizado
            FirebaseManager.Instance.TriggerFeedbackMessage("Por favor, introduce tu contrase�a para confirmar.");
            return;
        }

        // Desactivamos el bot�n para evitar clics m�ltiples
        if (deleteButton != null)
        {
            deleteButton.interactable = false;
        }

        // Llamamos al m�todo que creamos en el FirebaseManager
        _ = FirebaseManager.Instance.DeleteUserAccount(password);

        // Reactivamos el bot�n despu�s de un momento para que el usuario pueda reintentar si falla
        // El feedback de �xito o error se manejar� por el evento OnFeedbackMessage.
        // Podr�amos conectarnos a ese evento aqu� tambi�n si quisi�ramos.
        Invoke(nameof(ReEnableButton), 2f);
    }

    private void ReEnableButton()
    {
        if (deleteButton != null)
        {
            deleteButton.interactable = true;
        }
    }
}