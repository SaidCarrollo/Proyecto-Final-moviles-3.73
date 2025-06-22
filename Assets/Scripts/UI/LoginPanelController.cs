using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LoginPanelController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("El campo de texto para el email que está en esta escena.")]
    [SerializeField] private TMP_InputField emailInputField;
    [Tooltip("El campo de texto para la contraseña que está en esta escena.")]
    [SerializeField] private TMP_InputField passwordInputField;
    [Tooltip("El texto para mostrar mensajes de feedback (errores, éxito, etc.).")]
    [SerializeField] private TMP_Text feedbackText;
    [Tooltip("El botón de login de esta escena.")]
    [SerializeField] private Button loginButton;

    private void OnEnable()
    {
        // Nos suscribimos al evento de feedback del FirebaseManager
        FirebaseManager.OnFeedbackMessage += HandleFeedbackMessage;
    }

    private void OnDisable()
    {
        // Siempre nos desuscribimos para evitar errores.
        FirebaseManager.OnFeedbackMessage -= HandleFeedbackMessage;
    }

    /// <summary>
    /// Este es el método que llamarás desde el evento OnClick del botón de Login.
    /// </summary>
    public void HandleLoginRequest()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            HandleFeedbackMessage("Por favor, completa todos los campos.");
            return;
        }

        // Desactivamos el botón para evitar múltiples clics mientras se procesa.
        if (loginButton != null) loginButton.interactable = false;

        // Llamamos al método del manager pasándole los datos.
        _ = FirebaseManager.Instance.Login(email, password);
    }

    /// <summary>
    /// Escucha los mensajes del FirebaseManager y los muestra en la UI.
    /// </summary>
    private void HandleFeedbackMessage(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }

        // Reactivamos el botón después de recibir un mensaje (ej. un error).
        // Podrías añadir lógica más compleja para no reactivarlo en caso de éxito.
        if (loginButton != null) loginButton.interactable = true;
    }
}