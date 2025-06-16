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

    // --- Evento Est�tico para la UI (se mantiene) ---
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

    // --- CORRECCI�N 1: VOLVEMOS A async void Start() ---
    // Esto asegura que Firebase se inicialice por completo ANTES de que cualquier otra cosa intente usarlo.
    // Es la forma m�s simple y segura de manejar la inicializaci�n.
    async void Start()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync();

        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        // Suscribimos el listener que notificar� a la UI
        auth.StateChanged += OnAuthStateChanged;

        // Comprobamos si ya hab�a una sesi�n iniciada (como en tu c�digo original)
        if (auth.CurrentUser != null)
        {
            Debug.Log($"Usuario {auth.CurrentUser.Email} ya tiene sesi�n iniciada. Cargando progreso...");
            // Asignamos el usuario y cargamos su progreso
            CurrentUser = auth.CurrentUser;
            await LoadPlayerProgress(CurrentUser.UserId);

            // Notificamos a la UI que el estado ha cambiado
            OnAuthStateChanged_Custom?.Invoke();
        }
        else
        {
            Debug.Log("No hay sesi�n activa. Se requiere inicio de sesi�n manual.");
        }
    }

    // --- CORRECCI�N 2: OnLoginButtonClicked RESTAURADO Y MEJORADO ---
    // Recuperamos la l�gica directa que funcionaba y la integramos con el nuevo sistema.
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

        try
        {
            // Hacemos el login
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            CurrentUser = result.User; // Asignamos el usuario
            Debug.Log($"�Login exitoso! User ID: {CurrentUser.UserId}");

            // Llamamos a cargar el progreso, que tambi�n se encarga del feedback
            await LoadPlayerProgress(CurrentUser.UserId);
        }
        catch (FirebaseException ex)
        {
            CurrentUser = null; // Nos aseguramos de que no quede un usuario viejo
            Debug.LogError($"Error de login: {ex.Message}");
            // Mostramos el feedback de error (esto ya funcionaba bien)
            if (feedbackText != null) feedbackText.text = GetFriendlyErrorMessage(ex);

            // Notificamos a la UI que el login fall� (el usuario es null)
            OnAuthStateChanged_Custom?.Invoke();
        }
    }

    // --- CORRECCI�N 3: LoadPlayerProgress RECUPERA EL FEEDBACK DE �XITO ---
    public async Task LoadPlayerProgress(string userId)
    {
        DocumentReference userDoc = db.Collection("users").Document(userId);
        DocumentSnapshot snapshot = await userDoc.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            int highestLevel = snapshot.GetValue<int>("highestLevelUnlocked");
            Debug.Log($"Progreso cargado. El nivel m�s alto DESBLOQUEADO es: {highestLevel+1}");

            // RESTAURADO: El feedback de �xito vuelve a estar aqu�.
            if (feedbackText != null) feedbackText.text = "�Bienvenido de nuevo!";

            // RESTAURADO: Disparamos el evento para que los niveles se desbloqueen.
            OnLoginSuccess.Invoke(highestLevel);
        }
        else
        {
            Debug.LogWarning($"No se encontr� documento de datos para el usuario {userId}. Usando nivel 1 por defecto.");
            OnLoginSuccess.Invoke(0); // El nivel m�s alto desbloqueado es 0 (solo el 1 est� disponible)
        }
    }

    // --- OnAuthStateChanged SIMPLIFICADO ---
    // Ahora solo se encarga de reaccionar a cambios externos, como el SignOut.
    private void OnAuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != CurrentUser)
        {
            CurrentUser = auth.CurrentUser;
            Debug.Log("El estado de autenticaci�n cambi� (probablemente por SignOut). Notificando a la UI...");
            OnAuthStateChanged_Custom?.Invoke();
        }
    }

    // El resto de m�todos no necesitan cambios
    public void SignOut()
    {
        if (auth.CurrentUser != null)
        {
            auth.SignOut(); // Esto disparar� OnAuthStateChanged, que actualizar� la UI
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
            case AuthError.WrongPassword: return "La contrase�a es incorrecta.";
            case AuthError.UserNotFound: return "No se encontr� un usuario con ese email.";
            case AuthError.InvalidEmail: return "El formato del email no es v�lido.";
            default: return "Error al iniciar sesi�n. Int�ntalo de nuevo.";
        }
    }
}