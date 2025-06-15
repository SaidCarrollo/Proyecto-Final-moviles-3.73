using UnityEngine;
using TMPro; // Necesario para TextMeshPro
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine.Events; // Necesario para UnityEvent

// Creamos un evento personalizado para pasar el nivel cargado a otros scripts.
// Esto es una buena práctica para que los scripts no dependan directamente unos de otros.
[System.Serializable]
public class OnLoginSuccessEvent : UnityEvent<int> { }

public class LoginManager : MonoBehaviour
{
    // --- Referencias a la UI ---
    [Header("UI Elements")]
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_Text feedbackText;

    [Header("Events")]
    public OnLoginSuccessEvent OnLoginSuccess; // Este evento se disparará con el nivel del jugador

    // --- Firebase ---
    private FirebaseAuth auth;
    private FirebaseFirestore db;

    void Start()
    {
        // Inicializa Firebase
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    // --- Método que llamará el botón de Login ---
    public async void OnLoginButtonClicked()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            if (feedbackText != null) feedbackText.text = "Por favor, completa todos los campos.";
            return;
        }

        if (feedbackText != null) feedbackText.text = "Iniciando sesión...";
        await LoginUserAsync(email, password);
    }

    private async Task LoginUserAsync(string email, string password)
    {
        try
        {
            // 1. Iniciar sesión en Firebase Authentication
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            FirebaseUser user = result.User;

            Debug.Log($"¡Login exitoso! User ID: {user.UserId}");
            if (feedbackText != null) feedbackText.text = "¡Login exitoso! Cargando progreso...";

            // 2. Cargar los datos del jugador desde Firestore
            await LoadPlayerProgress(user.UserId);
        }
        catch (FirebaseException ex)
        {
            // Manejar errores comunes de login
            AuthError errorCode = (AuthError)ex.ErrorCode;
            string errorMessage = "Error de login desconocido.";

            switch (errorCode)
            {
                case AuthError.WrongPassword:
                    errorMessage = "Contraseña incorrecta.";
                    break;
                case AuthError.UserNotFound:
                    errorMessage = "No se encontró un usuario con ese correo.";
                    break;
                case AuthError.InvalidEmail:
                    errorMessage = "El formato del correo no es válido.";
                    break;
            }

            Debug.LogError($"Error de login: {errorMessage}");
            if (feedbackText != null) feedbackText.text = errorMessage;
        }
    }

    private async Task LoadPlayerProgress(string userId)
    {
        DocumentReference userDoc = db.Collection("users").Document(userId);
        DocumentSnapshot snapshot = await userDoc.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            // Intentamos obtener el valor. Si no existe, usamos 1 como valor por defecto.
            int highestLevel = snapshot.GetValue<int>("highestLevelUnlocked");
            Debug.Log($"Progreso cargado. Nivel más alto desbloqueado: {highestLevel+1}");
            if (feedbackText != null) feedbackText.text = $"¡Bienvenido! Nivel {highestLevel+1} desbloqueado.";

            // --- ¡AQUÍ ESTÁ LA MAGIA! ---
            // Disparamos el evento y le pasamos el nivel que cargamos.
            // Cualquier otro script (como tu LevelSelectorManager) puede escuchar este evento.
            OnLoginSuccess.Invoke(highestLevel);
        }
        else
        {
            Debug.LogWarning($"No se encontró documento de datos para el usuario {userId}. Usando nivel 1 por defecto.");
            // Si no hay datos (muy raro si el registro siempre los crea), usamos 1.
            OnLoginSuccess.Invoke(1);
        }
    }
}