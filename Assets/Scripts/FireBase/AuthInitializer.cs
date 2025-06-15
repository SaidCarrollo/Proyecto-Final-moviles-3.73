using UnityEngine;
using Firebase.Auth;

public class AuthInitializer : MonoBehaviour
{
    void Start()
    {
        // Comprobar si Firebase est� listo
        if (FirebaseAuth.DefaultInstance == null)
        {
            Debug.LogError("Firebase Auth no est� listo.");
            // Muestra la pantalla de Login/Registro
            return;
        }

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser != null)
        {
            // --- �HAY UN USUARIO LOGUEADO! ---
            Debug.Log($"Usuario {auth.CurrentUser.Email} ya tiene sesi�n iniciada.");
            // Aqu� deber�as cargar su progreso y llevarlo directamente al men� de niveles,
            // salt�ndote la pantalla de Login/Registro.
            // Por ejemplo, llamar�as a LoadPlayerProgress(auth.CurrentUser.UserId);
        }
        else
        {
            // --- NO HAY NADIE LOGUEADO ---
            Debug.Log("No hay sesi�n activa. Mostrando pantalla de Login/Registro.");
            // Muestra la pantalla de Login/Registro como lo har�as normalmente.
        }
    }
}