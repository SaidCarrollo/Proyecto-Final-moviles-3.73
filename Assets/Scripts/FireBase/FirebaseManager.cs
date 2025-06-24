using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System;


public class FirebaseManager : SingletonPersistent<FirebaseManager>
{
    public static event Action<string> OnFeedbackMessage;

    // --- Evento Estático para la UI (se mantiene) ---
    public static event Action OnAuthStateChanged_Custom;

    // --- Propiedades de Firebase y Usuario (se mantienen) ---
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    public FirebaseUser CurrentUser { get; private set; }

    // --- CORRECCIÓN 1: VOLVEMOS A async void Start() ---
    // Esto asegura que Firebase se inicialice por completo ANTES de que cualquier otra cosa intente usarlo.
    // Es la forma más simple y segura de manejar la inicialización.
    async void Start()
    {
        // La inicialización se mantiene igual
        await FirebaseApp.CheckAndFixDependenciesAsync();
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        auth.StateChanged += OnAuthStateChanged; // El listener oficial es clave

        // Comprobamos si ya había una sesión iniciada
        if (auth.CurrentUser != null)
        {
            // El listener OnAuthStateChanged se encargará de actualizar el estado
            // y notificar a la UI, por lo que aquí solo necesitamos loguear.
            Debug.Log($"Sesión persistente detectada para {auth.CurrentUser.Email}.");
        }
        else
        {
            Debug.Log("No hay sesión activa. Se requiere inicio de sesión manual.");
            // Si no hay usuario, disparamos el evento para que la UI se ponga en modo "invitado".
            OnAuthStateChanged_Custom?.Invoke();
        }
    }



    // --- CORRECCIÓN 2: OnLoginButtonClicked RESTAURADO Y MEJORADO ---
    // Recuperamos la lógica directa que funcionaba y la integramos con el nuevo sistema.
    public async Task Login(string email, string password)
    {
        OnFeedbackMessage?.Invoke("Iniciando sesión...");

        try
        {
            await auth.SignInWithEmailAndPasswordAsync(email, password);
            // El resto del flujo lo maneja OnAuthStateChanged automáticamente
        }
        catch (FirebaseException ex)
        {
            Debug.LogError($"Error de login: {ex.Message}");
            // Usamos el nuevo evento para enviar el mensaje de error a la UI
            OnFeedbackMessage?.Invoke(GetFriendlyErrorMessage(ex));
        }
    }

    // --- CORRECCIÓN 3: LoadPlayerProgress RECUPERA EL FEEDBACK DE ÉXITO ---
    public async Task LoadPlayerProgress(string userId)
    {
        DocumentReference userDoc = db.Collection("users").Document(userId);
        DocumentSnapshot snapshot = await userDoc.GetSnapshotAsync();
        int highestLevel = 0; // Valor por defecto

        if (snapshot.Exists)
        {
            highestLevel = snapshot.GetValue<int>("highestLevelUnlocked");
            Debug.Log($"Progreso de Firebase cargado. Nivel más alto: {highestLevel}");
        }
        else
        {
            Debug.LogWarning($"No se encontró documento en Firebase. Usando nivel por defecto 0.");
        }

        // Delegamos la lógica de sincronización al ProgressManager
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.SyncWithOnlineData(highestLevel);
        }

        // El evento OnLoginSuccess puede seguir siendo útil para otras cosas, como la UI.
        // Lo llamamos con el progreso final después de la sincronización.
        OnFeedbackMessage?.Invoke("¡Bienvenido de nuevo!");
    }
    public async void SaveProgressToFirestore(int progress)
    {
        if (CurrentUser == null) return;

        DocumentReference userDoc = db.Collection("users").Document(CurrentUser.UserId);
        var data = new { highestLevelUnlocked = progress };

        await userDoc.SetAsync(data, SetOptions.MergeAll);
        Debug.Log($"Progreso {progress} guardado en Firestore.");
    }


    private void OnAuthStateChanged(object sender, EventArgs eventArgs)
    {
        // Este es el método más importante. Es la ÚNICA fuente de verdad sobre el estado del usuario.
        // Se llama automáticamente por Firebase al iniciar, al loguearse y al desloguearse.
        if (auth.CurrentUser != CurrentUser)
        {
            bool wasLoggedIn = CurrentUser != null;
            CurrentUser = auth.CurrentUser;

            if (!wasLoggedIn && CurrentUser != null)
            {
                Debug.Log($"LOGIN DETECTADO: {CurrentUser.Email}");
                // Si acabamos de loguearnos, cargamos su progreso.
                _ = LoadPlayerProgress(CurrentUser.UserId); // El _ descarta el warning de no usar await
            }
            else if (wasLoggedIn && CurrentUser == null)
            {
                Debug.Log("LOGOUT DETECTADO.");
                ProgressManager.Instance?.LoadLocalProgress(); // Opcional, para recargar estado local
                                                               //      OnAuthStateChanged_Custom?.Invoke();
            }

            // Notificamos a toda la UI que el estado ha cambiado.
            OnAuthStateChanged_Custom?.Invoke();
        }
    }
    // Este nuevo método permite que la UI se "sincronice" cuando quiera.
    public void RequestUIUpdate()
    {
        Debug.Log("Solicitud de actualización de UI recibida. Invocando evento.");
        OnAuthStateChanged_Custom?.Invoke();
    }


    // Necesitamos un cambio en SignOut para limpiar el progreso guardado
    public void SignOut()
    {
        if (auth.CurrentUser != null)
        {
            auth.SignOut();
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
    // --- MÉTODO NUEVO PARA ELIMINAR CUENTA ---
    public async Task DeleteUserAccount(string password)
    {
        // 1. Comprobar si hay un usuario logueado
        if (CurrentUser == null)
        {
            OnFeedbackMessage?.Invoke("Debes iniciar sesión para eliminar tu cuenta.");
            return;
        }

        OnFeedbackMessage?.Invoke("Verificando credenciales...");
        FirebaseUser user = CurrentUser;

        try
        {
            // 2. Obtener credenciales para la re-autenticación (requerido por Firebase por seguridad)
            Credential credential = EmailAuthProvider.GetCredential(user.Email, password);
            await user.ReauthenticateAsync(credential);

            // 3. Si la re-autenticación es exitosa, eliminar los datos de Firestore
            OnFeedbackMessage?.Invoke("Eliminando datos del jugador...");
            DocumentReference userDoc = db.Collection("users").Document(user.UserId);
            await userDoc.DeleteAsync();
            Debug.Log($"Documento del usuario {user.UserId} eliminado de Firestore.");

            // 4. Eliminar el usuario de Firebase Authentication
            OnFeedbackMessage?.Invoke("Eliminando cuenta...");
            await user.DeleteAsync();

            Debug.Log("Cuenta de usuario eliminada permanentemente.");
            // Firebase llamará automáticamente a OnAuthStateChanged, que gestionará el logout y actualizará la UI.
            OnFeedbackMessage?.Invoke("Cuenta eliminada correctamente.");
        }
        catch (FirebaseException ex)
        {
            // 5. Manejar errores comunes
            AuthError errorCode = (AuthError)ex.ErrorCode;
            string errorMessage;
            switch (errorCode)
            {
                case AuthError.WrongPassword:
                    errorMessage = "La contraseña es incorrecta. No se puede eliminar la cuenta.";
                    break;
                case AuthError.RequiresRecentLogin:
                    errorMessage = "Por seguridad, debes volver a iniciar sesión antes de eliminar tu cuenta.";
                    break;
                default:
                    errorMessage = "Error al intentar eliminar la cuenta.";
                    break;
            }
            Debug.LogError($"Error al eliminar la cuenta: {ex.Message}");
            OnFeedbackMessage?.Invoke(errorMessage);
        }
    }
    // --- MÉTODO PÚBLICO PARA DISPARAR EL FEEDBACK DESDE FUERA ---
    public void TriggerFeedbackMessage(string message)
    {
        // Este método SÍ puede invocar el evento porque está dentro de la clase FirebaseManager.
        OnFeedbackMessage?.Invoke(message);
    }
}