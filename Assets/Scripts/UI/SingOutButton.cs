using UnityEngine;

public class SignOutButton : MonoBehaviour
{
    public void HandleSignOutClick()
    {
        // Busca la instancia del FirebaseManager
        FirebaseManager firebaseManager = FirebaseManager.Instance;

        if (firebaseManager != null)
        {
            // Llama al m�todo p�blico para cerrar sesi�n
            firebaseManager.SignOut();
        }
        else
        {
            Debug.LogError("FirebaseManager.Instance no encontrado. Aseg�rate de que exista en la escena.");
        }
    }
}