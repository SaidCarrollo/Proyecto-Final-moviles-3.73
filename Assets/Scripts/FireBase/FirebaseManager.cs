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

    // --- Evento Est�tico para la UI (se mantiene) ---
    public static event Action OnAuthStateChanged_Custom;

    // --- Propiedades de Firebase y Usuario (se mantienen) ---
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    public FirebaseUser CurrentUser { get; private set; }

    // --- CORRECCI�N 1: VOLVEMOS A async void Start() ---
    // Esto asegura que Firebase se inicialice por completo ANTES de que cualquier otra cosa intente usarlo.
    // Es la forma m�s simple y segura de manejar la inicializaci�n.
    async void Start()
    {
        // La inicializaci�n se mantiene igual
        await FirebaseApp.CheckAndFixDependenciesAsync();
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        auth.StateChanged += OnAuthStateChanged; // El listener oficial es clave

        // Comprobamos si ya hab�a una sesi�n iniciada
        if (auth.CurrentUser != null)
        {
            // El listener OnAuthStateChanged se encargar� de actualizar el estado
            // y notificar a la UI, por lo que aqu� solo necesitamos loguear.
            Debug.Log($"Sesi�n persistente detectada para {auth.CurrentUser.Email}.");
        }
        else
        {
            Debug.Log("No hay sesi�n activa. Se requiere inicio de sesi�n manual.");
            // Si no hay usuario, disparamos el evento para que la UI se ponga en modo "invitado".
            OnAuthStateChanged_Custom?.Invoke();
        }
    }



    // --- CORRECCI�N 2: OnLoginButtonClicked RESTAURADO Y MEJORADO ---
    // Recuperamos la l�gica directa que funcionaba y la integramos con el nuevo sistema.
    public async Task Login(string email, string password)
    {
        OnFeedbackMessage?.Invoke("Iniciando sesi�n...");

        try
        {
            await auth.SignInWithEmailAndPasswordAsync(email, password);
            // El resto del flujo lo maneja OnAuthStateChanged autom�ticamente
        }
        catch (FirebaseException ex)
        {
            Debug.LogError($"Error de login: {ex.Message}");
            // Usamos el nuevo evento para enviar el mensaje de error a la UI
            OnFeedbackMessage?.Invoke(GetFriendlyErrorMessage(ex));
        }
    }

    // --- CORRECCI�N 3: LoadPlayerProgress RECUPERA EL FEEDBACK DE �XITO ---
    public async Task LoadPlayerProgress(string userId)
    {
        DocumentReference userDoc = db.Collection("users").Document(userId);
        DocumentSnapshot snapshot = await userDoc.GetSnapshotAsync();
        int highestLevel = 0; // Valor por defecto

        if (snapshot.Exists)
        {
            highestLevel = snapshot.GetValue<int>("highestLevelUnlocked");
            Debug.Log($"Progreso de Firebase cargado. Nivel m�s alto: {highestLevel}");
        }
        else
        {
            Debug.LogWarning($"No se encontr� documento en Firebase. Usando nivel por defecto 0.");
        }

        // Delegamos la l�gica de sincronizaci�n al ProgressManager
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.SyncWithOnlineData(highestLevel);
        }

        // El evento OnLoginSuccess puede seguir siendo �til para otras cosas, como la UI.
        // Lo llamamos con el progreso final despu�s de la sincronizaci�n.
        OnFeedbackMessage?.Invoke("�Bienvenido de nuevo!");
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
        // Este es el m�todo m�s importante. Es la �NICA fuente de verdad sobre el estado del usuario.
        // Se llama autom�ticamente por Firebase al iniciar, al loguearse y al desloguearse.
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
    // Este nuevo m�todo permite que la UI se "sincronice" cuando quiera.
    public void RequestUIUpdate()
    {
        Debug.Log("Solicitud de actualizaci�n de UI recibida. Invocando evento.");
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
            case AuthError.WrongPassword: return "La contrase�a es incorrecta.";
            case AuthError.UserNotFound: return "No se encontr� un usuario con ese email.";
            case AuthError.InvalidEmail: return "El formato del email no es v�lido.";
            default: return "Error al iniciar sesi�n. Int�ntalo de nuevo.";
        }
    }
}