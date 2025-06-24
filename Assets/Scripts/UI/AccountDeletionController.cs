using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AccountDeletionController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("El campo para que el usuario ingrese su contraseña para confirmar.")]
    [SerializeField] private TMP_InputField passwordInputField;

    [Tooltip("El botón que inicia el proceso de eliminación.")]
    [SerializeField] private Button deleteButton;

    // No necesitas un campo de texto para feedback aquí, ya que reutilizaremos 
    // el sistema de feedback que ya tienes en LoginPanelController.

    /// <summary>
    /// Este método debe ser llamado desde el evento OnClick del botón de eliminar.
    /// </summary>
    public void HandleDeleteAccountRequest()
    {
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(password))
        {
            // Enviamos el mensaje a través del sistema de feedback centralizado
            FirebaseManager.Instance.TriggerFeedbackMessage("Por favor, introduce tu contraseña para confirmar.");
            return;
        }

        // Desactivamos el botón para evitar clics múltiples
        if (deleteButton != null)
        {
            deleteButton.interactable = false;
        }

        // Llamamos al método que creamos en el FirebaseManager
        _ = FirebaseManager.Instance.DeleteUserAccount(password);

        // Reactivamos el botón después de un momento para que el usuario pueda reintentar si falla
        // El feedback de éxito o error se manejará por el evento OnFeedbackMessage.
        // Podríamos conectarnos a ese evento aquí también si quisiéramos.
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