using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollowProjectile : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public float smoothSpeed = 5.0f;

    [Tooltip("El desplazamiento en X e Y desde el objetivo.")]
    public Vector2 offset = new Vector2(0f, 5f);

    [Tooltip("La posición Z inicial de la cámara. Este valor cambiará con el zoom.")]
    public float fixedZPosition = -20f;

    [Header("Zoom Settings")]
    [Tooltip("La velocidad a la que la cámara hace zoom. Ajusta este valor para un zoom más rápido o más lento.")]
    public float zoomSpeed = 2f;
    [Tooltip("El valor Z más cercano (más zoom). Debe ser un número negativo mayor que 'Max Zoom'.")]
    public float minZoom = -15f;
    [Tooltip("El valor Z más lejano (menos zoom). Debe ser un número negativo menor que 'Min Zoom'.")]
    public float maxZoom = -40f;

    [Header("Rotation Settings")]
    [Tooltip("Si se activa, la cámara rotará para mirar al objetivo. Desactívalo para una cámara 2D.")]
    public bool lookAtTarget = false;
    public float rotationSmoothSpeed = 5f;
    public bool simpleLookAt = true;

    private bool isFollowing = false;
    private Quaternion initialRotation;

    [Header("Fallback Target")]
    public Transform slingshotTransform;

    void Awake()
    {
        initialRotation = transform.rotation;
    }

    void Update()
    {
        HandleZoom();
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
                    desiredRotation = Quaternion.LookRotation(target.position - transform.position);
                }
                else
                {
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
                transform.rotation = initialRotation;
            }
        }
    }

    private void HandleZoom()
    {
        if (Touchscreen.current != null && Touchscreen.current.touches.Count == 2)
        {
            var touchZero = Touchscreen.current.touches[0];
            var touchOne = Touchscreen.current.touches[1];

            if (touchZero.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Moved && touchOne.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Moved)
            {
                return;
            }

            Vector2 touchZeroPrevPos = touchZero.position.ReadValue() - touchZero.delta.ReadValue();
            Vector2 touchOnePrevPos = touchOne.position.ReadValue() - touchOne.delta.ReadValue();

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position.ReadValue() - touchOne.position.ReadValue()).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            fixedZPosition += deltaMagnitudeDiff * zoomSpeed * 0.1f; 

            fixedZPosition = Mathf.Clamp(fixedZPosition, maxZoom, minZoom);
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
        FocusOnSlingshot();
    }

    public void FocusOnSlingshot()
    {
        if (slingshotTransform != null)
        {
            target = slingshotTransform;
            isFollowing = true;
            Debug.Log("Camera returning focus to the slingshot.");
        }
    }
}