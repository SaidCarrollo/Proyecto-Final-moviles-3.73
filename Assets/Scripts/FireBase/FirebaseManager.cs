using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System;

[System.Serializable]
public class OnLoginSuccessEvent : UnityEvent<int> { }

public class FirebaseManager : MonoBehaviour
{
    // --- Singleton, UI Refs, etc. (sin cambios) ---
    public static FirebaseManager Instance { get; private set; }
    [Header("UI Elements")]
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_Text feedbackText;
    [Header("Events")]
    public OnLoginSuccessEvent OnLoginSuccess;

    // --- Evento Estático para la UI (se mantiene) ---
    public static event Action OnAuthStateChanged_Custom;

    // --- Propiedades de Firebase y Usuario (se mantienen) ---
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    public FirebaseUser CurrentUser { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // --- CORRECCIÓN 1: VOLVEMOS A async void Start() ---
    // Esto asegura que Firebase se inicialice por completo ANTES de que cualquier otra cosa intente usarlo.
    // Es la forma más simple y segura de manejar la inicialización.
    async void Start()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync();

        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        // Suscribimos el listener que notificará a la UI
        auth.StateChanged += OnAuthStateChanged;

        // Comprobamos si ya había una sesión iniciada (como en tu código original)
        if (auth.CurrentUser != null)
        {
            Debug.Log($"Usuario {auth.CurrentUser.Email} ya tiene sesión iniciada. Cargando progreso...");
            // Asignamos el usuario y cargamos su progreso
            CurrentUser = auth.CurrentUser;
            await LoadPlayerProgress(CurrentUser.UserId);

            // Notificamos a la UI que el estado ha cambiado
            OnAuthStateChanged_Custom?.Invoke();
        }
        else
        {
            Debug.Log("No hay sesión activa. Se requiere inicio de sesión manual.");
        }
    }

    // --- CORRECCIÓN 2: OnLoginButtonClicked RESTAURADO Y MEJORADO ---
    // Recuperamos la lógica directa que funcionaba y la integramos con el nuevo sistema.
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

        try
        {
            // Hacemos el login
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            CurrentUser = result.User; // Asignamos el usuario
            Debug.Log($"¡Login exitoso! User ID: {CurrentUser.UserId}");

            // Llamamos a cargar el progreso, que también se encarga del feedback
            await LoadPlayerProgress(CurrentUser.UserId);
        }
        catch (FirebaseException ex)
        {
            CurrentUser = null; // Nos aseguramos de que no quede un usuario viejo
            Debug.LogError($"Error de login: {ex.Message}");
            // Mostramos el feedback de error (esto ya funcionaba bien)
            if (feedbackText != null) feedbackText.text = GetFriendlyErrorMessage(ex);

            // Notificamos a la UI que el login falló (el usuario es null)
            OnAuthStateChanged_Custom?.Invoke();
        }
    }

    // --- CORRECCIÓN 3: LoadPlayerProgress RECUPERA EL FEEDBACK DE ÉXITO ---
    public async Task LoadPlayerProgress(string userId)
    {
        DocumentReference userDoc = db.Collection("users").Document(userId);
        DocumentSnapshot snapshot = await userDoc.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            int highestLevel = snapshot.GetValue<int>("highestLevelUnlocked");
            Debug.Log($"Progreso cargado. El nivel más alto DESBLOQUEADO es: {highestLevel+1}");

            // RESTAURADO: El feedback de éxito vuelve a estar aquí.
            if (feedbackText != null) feedbackText.text = "¡Bienvenido de nuevo!";

            // RESTAURADO: Disparamos el evento para que los niveles se desbloqueen.
            OnLoginSuccess.Invoke(highestLevel);
        }
        else
        {
            Debug.LogWarning($"No se encontró documento de datos para el usuario {userId}. Usando nivel 1 por defecto.");
            OnLoginSuccess.Invoke(0); // El nivel más alto desbloqueado es 0 (solo el 1 está disponible)
        }
    }

    // --- OnAuthStateChanged SIMPLIFICADO ---
    // Ahora solo se encarga de reaccionar a cambios externos, como el SignOut.
    private void OnAuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != CurrentUser)
        {
            CurrentUser = auth.CurrentUser;
            Debug.Log("El estado de autenticación cambió (probablemente por SignOut). Notificando a la UI...");
            OnAuthStateChanged_Custom?.Invoke();
        }
    }

    // El resto de métodos no necesitan cambios
    public void SignOut()
    {
        if (auth.CurrentUser != null)
        {
            auth.SignOut(); // Esto disparará OnAuthStateChanged, que actualizará la UI
        }
    }

    void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= OnAuthStateChanged;
        }
    }

    private string GetFriendlyErrorMessage(FirebaseException ex)
    {
        AuthError errorCode = (AuthError)ex.ErrorCode;
        switch (errorCode)
        {
            case AuthError.WrongPassword: return "La contraseña es incorrecta.";
            case AuthError.UserNotFound: return "No se encontró un usuario con ese email.";
            case AuthError.InvalidEmail: return "El formato del email no es válido.";
            default: return "Error al iniciar sesión. Inténtalo de nuevo.";
        }
    }
}