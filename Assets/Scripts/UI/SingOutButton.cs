using UnityEngine;

public class SignOutButton : MonoBehaviour
{
    public void HandleSignOutClick()
    {
        // Busca la instancia del FirebaseManager
        FirebaseManager firebaseManager = FirebaseManager.Instance;

        if (firebaseManager != null)
        {
            // Llama al método público para cerrar sesión
            firebaseManager.SignOut();
        }
        else
        {
            Debug.LogError("FirebaseManager.Instance no encontrado. Asegúrate de que exista en la escena.");
        }
    }
}