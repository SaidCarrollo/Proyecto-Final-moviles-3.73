using UnityEngine;
using TMPro; // Necesario para usar TextMeshPro
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;

public class RegistrationManager : MonoBehaviour
{
    // --- Referencias a la UI ---
    // Asigna estos campos desde el Inspector de Unity
    [Header("UI Elements")]
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_Text feedbackText; // Un texto opcional para dar feedback al usuario (ej: "Registrando...", "Error...")

    // --- Firebase ---
    private FirebaseAuth auth;
    private FirebaseFirestore db;

    void Start()
    {
        // Inicializa Firebase
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    // --- Este es el método que llamará el botón ---
    public async void OnRegisterButtonClicked()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            if (feedbackText != null) feedbackText.text = "Por favor, completa todos los campos.";
            Debug.LogWarning("Email o contraseña vacíos.");
            return;
        }

        if (feedbackText != null) feedbackText.text = "Registrando...";
        Debug.Log("Intentando registrar nuevo usuario...");

        await RegisterUserAsync(email, password);
    }

    private async Task RegisterUserAsync(string email, string password)
    {
        try
        {
            // 1. Crear el usuario en Firebase Authentication
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            FirebaseUser newUser = result.User;

            Debug.Log($"¡Registro exitoso! User ID: {newUser.UserId}");
            if (feedbackText != null) feedbackText.text = $"¡Registro exitoso!";

            // 2. Guardar los datos iniciales en Firestore (como vimos antes)
            await SaveInitialUserData(newUser.UserId);
        }
        catch (FirebaseException ex)
        {
            // Manejar errores comunes de Firebase
            AuthError errorCode = (AuthError)ex.ErrorCode;
            string errorMessage = "Error de registro desconocido.";

            switch (errorCode)
            {
                case AuthError.WeakPassword:
                    errorMessage = "La contraseña es muy débil (debe tener al menos 6 caracteres).";
                    break;
                case AuthError.EmailAlreadyInUse:
                    errorMessage = "El correo electrónico ya está en uso.";
                    break;
                case AuthError.InvalidEmail:
                    errorMessage = "El formato del correo electrónico no es válido.";
                    break;
            }

            Debug.LogError($"Error de registro: {errorMessage}");
            if (feedbackText != null) feedbackText.text = errorMessage;
        }
    }

    private async Task SaveInitialUserData(string userId)
    {
        // Creamos el documento para el nuevo usuario en la colección "users"
        DocumentReference userDoc = db.Collection("users").Document(userId);

        // Definimos los datos iniciales. El nivel 1 es el primero desbloqueado.
        var initialData = new
        {
            highestLevelUnlocked = 0,
            email = emailInputField.text // También puedes guardar el email si lo deseas
        };

        // Guardamos los datos en Firestore
        await userDoc.SetAsync(initialData);
        Debug.Log($"Datos iniciales guardados para el usuario {userId}");
    }
}