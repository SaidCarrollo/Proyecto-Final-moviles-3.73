using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine.Events;

[System.Serializable]
public class OnLoginSuccessEvent : UnityEvent<int> { }

public class FirebaseManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    // Esto nos permite acceder a este script desde cualquier lugar con FirebaseManager.Instance
    public static FirebaseManager Instance { get; private set; }

    // --- Referencias a la UI (asígnalas en el Inspector) ---
    [Header("UI Elements")]
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_Text feedbackText;
    // (Aquí también podrías poner los campos para el registro)

    [Header("Events")]
    public OnLoginSuccessEvent OnLoginSuccess;

    // --- Firebase ---
    private FirebaseAuth auth;
    private FirebaseFirestore db;

    void Awake()
    {
        // Configuración del Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Hacemos que este objeto no se destruya al cambiar de escena
        }
    }

    async void Start()
    {
        // Espera a que Firebase esté listo
        await FirebaseApp.CheckAndFixDependenciesAsync();

        // Inicializa las instancias de Firebase
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        // --- LÓGICA DEL AUTHINITIALIZER (AHORA AQUÍ) ---
        if (auth.CurrentUser != null)
        {
            // ¡HAY UN USUARIO LOGUEADO!
            Debug.Log($"Usuario {auth.CurrentUser.Email} ya tiene sesión iniciada. Cargando progreso...");

            // Llamamos a la función para cargar su progreso.
            await LoadPlayerProgress(auth.CurrentUser.UserId);
        }
        else
        {
            // NO HAY NADIE LOGUEADO
            Debug.Log("No hay sesión activa. Se requiere inicio de sesión manual.");
            // Aquí te asegurarías de que la UI de Login/Registro esté visible.
        }
    }

    // --- Método para el botón de Login ---
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
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            FirebaseUser user = result.User;
            Debug.Log($"¡Login exitoso! User ID: {user.UserId}");

            await LoadPlayerProgress(user.UserId);
        }
        catch (FirebaseException ex)
        {
            // (Manejo de errores como antes)
            Debug.LogError($"Error de login: {ex.Message}");
            if (feedbackText != null) feedbackText.text = ex.Message;
        }
    }

    // Esta función ahora es pública para poder ser llamada si es necesario desde otro lugar,
    // pero principalmente se usa dentro de este script.
    public async Task LoadPlayerProgress(string userId)
    {
        DocumentReference userDoc = db.Collection("users").Document(userId);
        DocumentSnapshot snapshot = await userDoc.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            int highestLevel = snapshot.GetValue<int>("highestLevelUnlocked");
            // CORRECCIÓN en el log para mayor claridad:
            Debug.Log($"Progreso cargado. El nivel más alto COMPLETADO es: {highestLevel+1}");
            if (feedbackText != null) feedbackText.text = $"¡Bienvenido de nuevo!";

            // Disparamos el evento para que la UI se actualice
            OnLoginSuccess.Invoke(highestLevel);
        }
        else
        {
            Debug.LogWarning($"No se encontró documento de datos para el usuario {userId}. Usando nivel 1 por defecto.");
            OnLoginSuccess.Invoke(1);
        }
    }
}