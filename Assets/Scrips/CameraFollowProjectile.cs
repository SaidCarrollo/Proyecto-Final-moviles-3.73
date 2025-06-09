using UnityEngine;

public class CameraFollowProjectile : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public float smoothSpeed = 5.0f;

    [Tooltip("El desplazamiento en X e Y desde el objetivo.")]
    public Vector2 offset = new Vector2(0f, 5f);

    [Tooltip("La posici�n fija en el eje Z para la c�mara. Un valor negativo la aleja de la escena.")]
    public float fixedZPosition = -20f;

    [Header("Rotation Settings")]
    [Tooltip("Si se activa, la c�mara rotar� para mirar al objetivo. Desact�valo para una c�mara 2D.")]
    public bool lookAtTarget = false;
    public float rotationSmoothSpeed = 5f;

    public bool simpleLookAt = true;

    private bool isFollowing = false;
    private Quaternion initialRotation; 

    void Awake()
    {
        // Guarda la rotaci�n que la c�mara tiene al iniciar la escena.
        initialRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (target != null && isFollowing)
        {
            Vector3 desiredPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, fixedZPosition);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

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
                        desiredRotation = Quaternion.LookRotation(target.position - transform.position);
                    }
                }
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
            }
            else
            {
                // Para una c�mara 2D, mantenemos la rotaci�n inicial.
                transform.rotation = initialRotation;
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
    }
}