using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;

public class CameraFollowProjectile : MonoBehaviour
{
    [SerializeField] private bool isFollowingProjectile = false;
    private bool isPanning = false;

    public Transform target;
    public float smoothSpeed = 5.0f;
    public Vector2 offset = new Vector2(0f, 5f);

    public Transform slingshotTransform;
    public Vector2 closeUpOffset = new Vector2(0f, 5f);
    public Vector2 wideViewOffset = new Vector2(15f, 10f);
    private bool isWideView = false;

    public float panSpeed = 20f;
    private Vector2 panBoundsX;
    private Vector2 panBoundsY;
    private Vector3 lastPanPosition;

    public float zoomSpeedPerspective = 2f;
    public float minZoomZ = -15f;
    public float maxZoomZ = -40f;
    private float currentZPosition;

    public float zoomSpeedOrthographic = 1f;
    public float minZoomOrtho = 5f;
    public float maxZoomOrtho = 20f;

    public SpriteRenderer backgroundSprite;

    private Camera mainCamera;
    private Slingshot slingshot;
    private Quaternion initialRotation;

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            this.enabled = false;
            return;
        }

        if (backgroundSprite != null)
        {
            Bounds backgroundBounds = backgroundSprite.bounds;
            panBoundsX = new Vector2(backgroundBounds.min.x, backgroundBounds.max.x);
            panBoundsY = new Vector2(backgroundBounds.min.y, backgroundBounds.max.y);
        }
        else
        {
            Debug.LogError("Referencia a backgroundSprite no asignada en el Inspector. Deshabilitando script.");
            this.enabled = false;
            return;
        }

        initialRotation = transform.rotation;
        currentZPosition = transform.position.z;
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
            Vector3 desiredPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.fixedDeltaTime);
            transform.position = smoothedPosition;
            transform.rotation = initialRotation;
        }
    }
    void LateUpdate()
    {
        transform.position = ClampCameraPosition(transform.position);
    }
    private void HandleFreeRoam()
    {
        HandleZoom();
        HandlePanning();
    }
    public void ToggleCameraView()
    {
        isFollowingProjectile = false;
        target = null;
        isWideView = !isWideView;

        if (slingshotTransform != null)
        {
            StopAllCoroutines();
            Vector3 targetPosition;
            float targetZoom;

            if (isWideView)
            {
                targetPosition = slingshotTransform.position + (Vector3)wideViewOffset;
                targetZoom = mainCamera.orthographic ? maxZoomOrtho : maxZoomZ;
            }
            else
            {
                targetPosition = slingshotTransform.position + (Vector3)closeUpOffset;
                targetZoom = mainCamera.orthographic ? minZoomOrtho : minZoomZ;
            }

            targetPosition.x = Mathf.Clamp(targetPosition.x, panBoundsX.x, panBoundsX.y);
            targetPosition.y = Mathf.Clamp(targetPosition.y, panBoundsY.x, panBoundsY.y);

            StartCoroutine(MoveAndZoomToPosition(targetPosition, targetZoom));
        }
    }

    public void ResetToSlingshotView()
    {
        isFollowingProjectile = false;
        target = null;
        isWideView = false;

        if (slingshotTransform != null)
        {
            StopAllCoroutines();
            Vector3 targetPosition = slingshotTransform.position + (Vector3)closeUpOffset;
            float targetZoom = mainCamera.orthographic ? minZoomOrtho : minZoomZ;
            targetPosition.x = Mathf.Clamp(targetPosition.x, panBoundsX.x, panBoundsX.y);
            targetPosition.y = Mathf.Clamp(targetPosition.y, panBoundsY.x, panBoundsY.y);

            StartCoroutine(MoveAndZoomToPosition(targetPosition, targetZoom));
        }
    }

    private void HandleZoom()
    {
        if (slingshot != null && slingshot.isDragging) return;

        float scroll = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0;

        if (Touchscreen.current != null && Touchscreen.current.touches.Count == 2)
        {
            var touchZero = Touchscreen.current.touches[0];
            var touchOne = Touchscreen.current.touches[1];

            if (touchZero.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved || touchOne.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                Vector2 touchZeroPrevPos = touchZero.position.ReadValue() - touchZero.delta.ReadValue();
                Vector2 touchOnePrevPos = touchOne.position.ReadValue() - touchOne.delta.ReadValue();

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position.ReadValue() - touchOne.position.ReadValue()).magnitude;
                float difference = currentMagnitude - prevMagnitude;
                scroll = difference * 0.1f;
            }
        }

        if (Mathf.Abs(scroll) > 0.01f)
        {
            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize -= scroll * zoomSpeedOrthographic;
                mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoomOrtho, maxZoomOrtho);
            }
            else
            {
                currentZPosition += scroll * zoomSpeedPerspective;
                currentZPosition = Mathf.Clamp(currentZPosition, maxZoomZ, minZoomZ);
                Vector3 newPos = transform.position;
                newPos.z = currentZPosition;
                transform.position = newPos;
            }
        }
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
            Ray ray = Camera.main.ScreenPointToRay(inputPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 200f))
            {
                if (hit.collider.GetComponent<Slingshot>() == null && hit.collider.GetComponent<Projectile>() == null)
                {
                    isPanning = true;
                    lastPanPosition = Camera.main.ScreenToViewportPoint(inputPosition);
                }
            }
            else
            {
                isPanning = true;
                lastPanPosition = Camera.main.ScreenToViewportPoint(inputPosition);
            }
        }
        else if (primaryInputHeld && isPanning)
        {
            Vector3 delta = lastPanPosition - (Vector3)Camera.main.ScreenToViewportPoint(inputPosition);
            transform.Translate(delta.x * panSpeed, delta.y * panSpeed, 0);
            lastPanPosition = Camera.main.ScreenToViewportPoint(inputPosition);
        }

        if (primaryInputReleased)
        {
            isPanning = false;
        }
    }
    private Vector3 ClampCameraPosition(Vector3 position)
    {
        float camHeight;
        float camWidth;

        if (mainCamera.orthographic)
        {
            camHeight = mainCamera.orthographicSize;
            camWidth = camHeight * mainCamera.aspect;
        }
        else
        {
            float distance = Mathf.Abs(position.z);
            camHeight = distance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            camWidth = camHeight * mainCamera.aspect;
        }

        float minX = panBoundsX.x + camWidth;
        float maxX = panBoundsX.y - camWidth;
        float minY = panBoundsY.x + camHeight;
        float maxY = panBoundsY.y - camHeight;

        Vector3 clampedPos = position;

        if (minX > maxX)
        {
            clampedPos.x = (panBoundsX.x + panBoundsX.y) / 2;
        }
        else
        {
            clampedPos.x = Mathf.Clamp(position.x, minX, maxX);
        }

        if (minY > maxY)
        {
            clampedPos.y = (panBoundsY.x + panBoundsY.y) / 2;
        }
        else
        {
            clampedPos.y = Mathf.Clamp(position.y, minY, maxY);
        }

        return clampedPos;
    }
    public void StartFollowing(Transform newTarget)
    {
        target = newTarget;
        isFollowingProjectile = true;
        isWideView = false;
    }

    public void StopFollowing()
    {
        isFollowingProjectile = false;
        target = null;
        ResetToSlingshotView();
    }

    private IEnumerator MoveAndZoomToPosition(Vector3 targetPosition, float targetZoom)
    {
        float elapsedTime = 0f;
        float duration = 0.8f;
        Vector3 startingPos = transform.position;
        float startingZoom = mainCamera.orthographic ? mainCamera.orthographicSize : transform.position.z;

        while (elapsedTime < duration)
        {
            if (isPanning)
            {
                yield break;
            }

            Vector3 newPos = Vector3.Lerp(startingPos, targetPosition, (elapsedTime / duration));
            float newZoom = Mathf.Lerp(startingZoom, targetZoom, (elapsedTime / duration));

            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize = newZoom;
                newPos.z = transform.position.z;
            }
            else
            {
                newPos.z = newZoom;
            }

            transform.position = newPos;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (!isPanning)
        {
            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize = targetZoom;
                transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(targetPosition.x, targetPosition.y, targetZoom);
            }
        }
    }
}