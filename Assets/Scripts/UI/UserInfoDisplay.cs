using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class UserInfoDisplay : MonoBehaviour
{
    public string guestText = "Invitado";
    private TMP_Text textComponent;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        // Nos suscribimos a nuestro evento personalizado
        FirebaseManager.OnAuthStateChanged_Custom += UpdateUserInfo;
        // Actualizamos la UI en cuanto se activa
        UpdateUserInfo();
    }

    void OnDisable()
    {
        // Nos desuscribimos para evitar errores
        FirebaseManager.OnAuthStateChanged_Custom -= UpdateUserInfo;
    }

    // Este m�todo ya no necesita los argumentos 'sender' y 'e'
    private void UpdateUserInfo()
    {
        // La l�gica de comprobaci�n ahora es m�s simple y fiable
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.CurrentUser != null)
        {
            var user = FirebaseManager.Instance.CurrentUser;
            if (!string.IsNullOrEmpty(user.DisplayName)) { textComponent.text = user.DisplayName; }
            else if (!string.IsNullOrEmpty(user.Email)) { textComponent.text = user.Email; }
            else { textComponent.text = "Usuario"; }
        }
        else
        {
            textComponent.text = guestText;
        }
    }
}