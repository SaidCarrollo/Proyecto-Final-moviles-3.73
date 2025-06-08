using UnityEngine;

public class CameraFollowProjectile : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public float smoothSpeed = 5.0f;
    // AJUSTA ESTE OFFSET PARA UNA VISTA 2D (ej. m�s atr�s en Z, y quiz�s m�s alto en Y)
    // Ejemplo para un juego donde la acci�n principal ocurre en el plano XY mundial:
    public Vector3 offset = new Vector3(0f, 5f, -15f); // Aumenta el valor negativo de Z

    [Header("Rotation Settings")]
    // Para un estilo Angry Birds, a menudo es mejor desactivar la rotaci�n din�mica
    // o usar una rotaci�n m�s simple.
    public bool lookAtTarget = false; // DESACT�VALO para una c�mara fija estilo 2D
    public float rotationSmoothSpeed = 5f;

    // Si lookAtTarget es true, pero quieres una rotaci�n m�s simple (solo mirar al centro del target):
    public bool simpleLookAt = true;

    private bool isFollowing = false;
    private Quaternion initialRotation; // Para mantener la rotaci�n inicial si lookAtTarget es false

    void Awake()
    {
        initialRotation = transform.rotation; // Guarda la rotaci�n inicial
    }


    private void FixedUpdate()
    {
        if (target != null && isFollowing)
        {
            // --- Posicionamiento ---
            // El offset se aplica en el espacio del mundo directamente.
            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // --- Rotaci�n ---
            if (lookAtTarget)
            {
                Quaternion desiredRotation;
                if (simpleLookAt)
                {
                    // Simplemente mira hacia la posici�n del objetivo
                    desiredRotation = Quaternion.LookRotation(target.position - transform.position);
                }
                else
                {
                    // L�gica de rotaci�n original (mirar en direcci�n de la velocidad)
                    Rigidbody targetRb = target.GetComponent<Rigidbody>();
                    if (targetRb != null && targetRb.linearVelocity.sqrMagnitude > 0.01f)
                    {
                        desiredRotation = Quaternion.LookRotation(targetRb.linearVelocity.normalized);
                    }
                    else
                    {
                        // Si no hay velocidad, mira hacia el 'forward' del objetivo o su posici�n
                        // Para 2D, mirar hacia la posici�n podr�a ser mejor incluso aqu�
                        desiredRotation = Quaternion.LookRotation(target.position - transform.position);
                        // O Quaternion.LookRotation(target.forward);
                    }
                }
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
            }
            else
            {
                // Si no estamos mirando al objetivo, mantenemos la rotaci�n inicial o la que tenga la c�mara.
                // Para un estilo Angry Birds estricto donde la c�mara no rota, podr�as usar:
                // transform.rotation = initialRotation;
                // O simplemente no hacer nada aqu� si quieres que la c�mara mantenga cualquier rotaci�n
                // que se le haya dado en el editor y no la cambie din�micamente.
                // Si la c�mara fue orientada correctamente en el editor, no se necesita m�s.
            }
        }
    }
    public void StartFollowing(Transform newTarget)
    {
        target = newTarget;
        isFollowing = true;
        Debug.Log("Camera started following: " + newTarget.name);
    }

    public void StopFollowing()
    {
        isFollowing = false;
        Debug.Log("Camera stopped following.");
        // target = null; // Opcional
    }
}