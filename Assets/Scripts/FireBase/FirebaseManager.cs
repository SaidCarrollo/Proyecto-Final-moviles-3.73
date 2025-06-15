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

    // --- Referencias a la UI (as�gnalas en el Inspector) ---
    [Header("UI Elements")]
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_Text feedbackText;
    // (Aqu� tambi�n podr�as poner los campos para el registro)

    [Header("Events")]
    public OnLoginSuccessEvent OnLoginSuccess;

    // --- Firebase ---
    private FirebaseAuth auth;
    private FirebaseFirestore db;

    void Awake()
    {
        // Configuraci�n del Singleton
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
        // Espera a que Firebase est� listo
        await FirebaseApp.CheckAndFixDependenciesAsync();

        // Inicializa las instancias de Firebase
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        // --- L�GICA DEL AUTHINITIALIZER (AHORA AQU�) ---
        if (auth.CurrentUser != null)
        {
            // �HAY UN USUARIO LOGUEADO!
            Debug.Log($"Usuario {auth.CurrentUser.Email} ya tiene sesi�n iniciada. Cargando progreso...");

            // Llamamos a la funci�n para cargar su progreso.
            await LoadPlayerProgress(auth.CurrentUser.UserId);
        }
        else
        {
            // NO HAY NADIE LOGUEADO
            Debug.Log("No hay sesi�n activa. Se requiere inicio de sesi�n manual.");
            // Aqu� te asegurar�as de que la UI de Login/Registro est� visible.
        }
    }

    // --- M�todo para el bot�n de Login ---
    public async void OnLoginButtonClicked()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            if (feedbackText != null) feedbackText.text = "Por favor, completa todos los campos.";
            return;
        }

        if (feedbackText != null) feedbackText.text = "Iniciando sesi�n...";
        await LoginUserAsync(email, password);
    }

    private async Task LoginUserAsync(string email, string password)
    {
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            FirebaseUser user = result.User;
            Debug.Log($"�Login exitoso! User ID: {user.UserId}");

            await LoadPlayerProgress(user.UserId);
        }
        catch (FirebaseException ex)
        {
            // (Manejo de errores como antes)
            Debug.LogError($"Error de login: {ex.Message}");
            if (feedbackText != null) feedbackText.text = ex.Message;
        }
    }

    // Esta funci�n ahora es p�blica para poder ser llamada si es necesario desde otro lugar,
    // pero principalmente se usa dentro de este script.
    public async Task LoadPlayerProgress(string userId)
    {
        DocumentReference userDoc = db.Collection("users").Document(userId);
        DocumentSnapshot snapshot = await userDoc.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            int highestLevel = snapshot.GetValue<int>("highestLevelUnlocked");
            // CORRECCI�N en el log para mayor claridad:
            Debug.Log($"Progreso cargado. El nivel m�s alto COMPLETADO es: {highestLevel+1}");
            if (feedbackText != null) feedbackText.text = $"�Bienvenido de nuevo!";

            // Disparamos el evento para que la UI se actualice
            OnLoginSuccess.Invoke(highestLevel);
        }
        else
        {
            Debug.LogWarning($"No se encontr� documento de datos para el usuario {userId}. Usando nivel 1 por defecto.");
            OnLoginSuccess.Invoke(1);
        }
    }
}