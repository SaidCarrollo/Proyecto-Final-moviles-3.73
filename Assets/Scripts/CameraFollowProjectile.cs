using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CameraFollowProjectile : MonoBehaviour
{
    [Header("Estado Actual")]
    [SerializeField] private bool isFollowingProjectile = false;

    [Header("Configuración de Seguimiento")]
    public Transform target;
    public float smoothSpeed = 5.0f;
    public Vector2 offset = new Vector2(0f, 5f);
    public float fixedZPosition = -20f;

    [Header("Configuración de Exploración Libre")]
    [Tooltip("Velocidad a la que la cámara se mueve al arrastrar.")]
    public float panSpeed = 20f;
    [Tooltip("Límites del movimiento en el eje X (min, max).")]
    public Vector2 panBoundsX = new Vector2(-10f, 50f);
    [Tooltip("Límites del movimiento en el eje Y (min, max).")]
    public Vector2 panBoundsY = new Vector2(0f, 20f);
    private Vector3 lastPanPosition;
    private bool isPanning = false;
    [SerializeField] Slingshot slingshot; 

    [Header("Configuración de Zoom")]
    public float zoomSpeed = 2f;
    public float minZoom = -15f;
    public float maxZoom = -40f;

    [Header("Referencias")]
    public Transform slingshotTransform;

    private Quaternion initialRotation;

    void Awake()
    {
        initialRotation = transform.rotation;
    }

    void Update()
    {

        if (isFollowingProjectile)
        {
            HandleZoom();
            return;
        }

        HandleFreeRoam();
    }

    private void FixedUpdate()
    {
        if (target != null && isFollowingProjectile)
        {
            Vector3 desiredPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, fixedZPosition);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.rotation = initialRotation;
        }
    }

    private void HandleFreeRoam()
    {
        HandleZoom();
        HandlePanning();
    }

    private void HandlePanning()
    {
        if (slingshot != null && slingshot.isDragging)
        {
            isPanning = false;
            return;
        }
        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (isPanning && (Touchscreen.current?.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended || (Mouse.current?.leftButton.wasReleasedThisFrame ?? false)))
            {
                isPanning = false;
            }
            return;
        }

        bool primaryInputPressed = false;
        bool primaryInputHeld = false;
        bool primaryInputReleased = false;
        Vector2 inputPosition = Vector2.zero;
        bool inputActive = false;

        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            var touch = Touchscreen.current.primaryTouch;
            inputPosition = touch.position.ReadValue();
            var touchPhase = touch.phase.ReadValue();

            if (touchPhase != UnityEngine.InputSystem.TouchPhase.None && touchPhase != UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                inputActive = true;
                primaryInputPressed = touchPhase == UnityEngine.InputSystem.TouchPhase.Began;
                primaryInputHeld = touchPhase == UnityEngine.InputSystem.TouchPhase.Moved || touchPhase == UnityEngine.InputSystem.TouchPhase.Stationary;
                primaryInputReleased = touchPhase == UnityEngine.InputSystem.TouchPhase.Ended;

            }
        }
        else if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.isPressed || Mouse.current.leftButton.wasReleasedThisFrame)
            {
                inputActive = true;
                inputPosition = Mouse.current.position.ReadValue();
                primaryInputPressed = Mouse.current.leftButton.wasPressedThisFrame;
                primaryInputHeld = Mouse.current.leftButton.isPressed;
                primaryInputReleased = Mouse.current.leftButton.wasReleasedThisFrame;

            }
        }

        if (primaryInputPressed && !isPanning)
        {
            isPanning = true;
            lastPanPosition = Camera.main.ScreenToViewportPoint(inputPosition);
        }
        else if (primaryInputHeld && isPanning)
        {
            Vector3 delta = lastPanPosition - (Vector3)Camera.main.ScreenToViewportPoint(inputPosition);
            transform.Translate(delta.x * panSpeed, delta.y * panSpeed, 0);

            Vector3 clampedPos = transform.position;
            clampedPos.x = Mathf.Clamp(clampedPos.x, panBoundsX.x, panBoundsX.y);
            clampedPos.y = Mathf.Clamp(clampedPos.y, panBoundsY.x, panBoundsY.y);
            transform.position = clampedPos;

            lastPanPosition = Camera.main.ScreenToViewportPoint(inputPosition);
        }

        if (primaryInputReleased)
        {
            isPanning = false;
        }
    }
    private void HandleZoom()
    {
        if (slingshot != null && slingshot.isDragging)
        {
            return;
        }

        float scroll = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0;

        if (Touchscreen.current != null && Touchscreen.current.touches.Count == 2)
        {
            var touchZero = Touchscreen.current.touches[0];
            var touchOne = Touchscreen.current.touches[1];

            if (touchZero.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved || touchOne.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                Vector2 touchZeroPrevPos = touchZero.position.ReadValue() - touchZero.delta.ReadValue();
                Vector2 touchOnePrevPos = touchOne.position.ReadValue() - touchOne.delta.ReadValue();

                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position.ReadValue() - touchOne.position.ReadValue()).magnitude;

                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                fixedZPosition += deltaMagnitudeDiff * (zoomSpeed / 10f);

                Debug.Log($"<color=magenta>ZOOM DETECTADO:</color> Delta: {deltaMagnitudeDiff}, Nuevo Z: {fixedZPosition}");
            }
        }
        else if (scroll != 0)
        {
            fixedZPosition += scroll * zoomSpeed;
            Debug.Log($"<color=magenta>SCROLL DETECTADO:</color> Scroll: {scroll}, Nuevo Z: {fixedZPosition}");
        }

        fixedZPosition = Mathf.Clamp(fixedZPosition, maxZoom, minZoom);

        Vector3 newPos = transform.position;
        newPos.z = fixedZPosition;
        transform.position = newPos;
    }

    public void StartFollowing(Transform newTarget)
    {
        target = newTarget;
        isFollowingProjectile = true;
        Debug.Log("Cámara: Iniciando seguimiento de " + newTarget.name);
    }

    public void StopFollowing()
    {
        isFollowingProjectile = false;
        target = null;
        Debug.Log("Cámara: Deteniendo seguimiento.");
        ResetToSlingshotView();
    }

    public void ResetToSlingshotView()
    {
        isFollowingProjectile = false;
        target = null;

        if (slingshotTransform != null)
        {
            Debug.Log("Cámara: Enfocando en la resortera.");
            StopAllCoroutines();
            StartCoroutine(MoveToPosition(slingshotTransform.position));
        }
    }

    private System.Collections.IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        float duration = 0.8f; // Aumenté un poco la duración para que el efecto sea más suave
        Vector3 startingPos = transform.position;
        Vector3 endPos = new Vector3(targetPosition.x + offset.x, targetPosition.y + offset.y, transform.position.z);

        while (elapsedTime < duration)
        {
            if (isPanning)
            {
                Debug.Log("Enfoque automático cancelado por el usuario.");
                yield break; 
            }

            transform.position = Vector3.Lerp(startingPos, endPos, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (!isPanning)
        {
            transform.position = endPos;
        }
    }
}