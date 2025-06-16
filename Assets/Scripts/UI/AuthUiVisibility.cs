using UnityEngine;
using UnityEngine.UI;

public class AuthUIVisibility : MonoBehaviour
{
    [Tooltip("Marca esta casilla para que el objeto se muestre SÓLO cuando el usuario NO está logueado.")]
    public bool showWhenLoggedOut = false;

    void OnEnable()
    {
        // Nos suscribimos a nuestro evento centralizado
        FirebaseManager.OnAuthStateChanged_Custom += UpdateVisibility;
        // Actualizamos la visibilidad en cuanto se activa
        UpdateVisibility();
    }

    void OnDisable()
    {
        // Nos desuscribimos para evitar errores
        FirebaseManager.OnAuthStateChanged_Custom -= UpdateVisibility;
    }

    private void UpdateVisibility()
    {
        bool isLoggedIn = FirebaseManager.Instance != null && FirebaseManager.Instance.CurrentUser != null;

        if (showWhenLoggedOut)
        {
            // Si queremos mostrarlo cuando está deslogueado, la visibilidad es la INVERSA de isLoggedIn
            gameObject.SetActive(!isLoggedIn);
        }
        else
        {
            // Si queremos mostrarlo cuando está logueado, la visibilidad es la MISMA que isLoggedIn
            gameObject.SetActive(isLoggedIn);
        }
    }
}