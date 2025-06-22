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

    // --- MÉTODO OnEnable MODIFICADO ---
    void OnEnable()
    {
        // Siempre nos suscribimos para reaccionar a futuros cambios (como logout).
        FirebaseManager.OnAuthStateChanged_Custom += UpdateUserInfo;

        // Y le pedimos al Manager que nos envíe el estado actual.
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.RequestUIUpdate();
        }
    }

    void OnDisable()
    {
        // Siempre nos desuscribimos.
        FirebaseManager.OnAuthStateChanged_Custom -= UpdateUserInfo;
    }


    private void UpdateUserInfo()
    {
        // La lógica interna de este método ya era correcta.
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