using UnityEngine;
using Firebase.Auth;

public class AuthInitializer : MonoBehaviour
{
    void Start()
    {
        // Comprobar si Firebase está listo
        if (FirebaseAuth.DefaultInstance == null)
        {
            Debug.LogError("Firebase Auth no está listo.");
            // Muestra la pantalla de Login/Registro
            return;
        }

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser != null)
        {
            // --- ¡HAY UN USUARIO LOGUEADO! ---
            Debug.Log($"Usuario {auth.CurrentUser.Email} ya tiene sesión iniciada.");
            // Aquí deberías cargar su progreso y llevarlo directamente al menú de niveles,
            // saltándote la pantalla de Login/Registro.
            // Por ejemplo, llamarías a LoadPlayerProgress(auth.CurrentUser.UserId);
        }
        else
        {
            // --- NO HAY NADIE LOGUEADO ---
            Debug.Log("No hay sesión activa. Mostrando pantalla de Login/Registro.");
            // Muestra la pantalla de Login/Registro como lo harías normalmente.
        }
    }
}