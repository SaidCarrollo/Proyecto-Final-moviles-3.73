using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Slingshot : MonoBehaviour
{
    [Header("Configuración General")]
    public float maxStretch = 3.0f;
    public float launchForceMultiplier = 100f;
    public int totalProjectiles = 5;
    public float timeToPrepareNext = 0.5f;

    [Header("Referencias Esenciales")]
    public Transform anchorPoint;
    public Transform spawnPoint;

    [Header("Gestión de Tipos de Proyectiles")]
    public GameObject[] projectilePrefabs_TypeSequence;
    private int currentProjectileTypeIndex = 0;

    [Header("Bandas Visuales (Opcional)")]
    public LineRenderer bandLeft;
    public LineRenderer bandRight;

    private bool isDragging = false;
    private bool primaryInputStartedThisFrame = false;
    private bool primaryInputIsHeld = false;
    private Vector2 currentInputScreenPosition;

    private GameObject currentProjectile;
    private Rigidbody currentProjectileRb;
    private SpringJoint currentSpringJoint;
    private int projectilesRemaining_TotalLaunches;

    private Collider objectCollider;
    private CameraFollowProjectile cameraFollowScript;

    void Start()
    {
        objectCollider = GetComponent<Collider>();
        if (objectCollider == null)
        {
            Debug.LogWarning("Slingshot GameObject no tiene un Collider. La detección de inicio de arrastre podría no funcionar como se espera si se depende de un clic/toque directo sobre la resortera.");
        }
        if (projectilePrefabs_TypeSequence == null || projectilePrefabs_TypeSequence.Length == 0)
        {
            Debug.LogError("¡ERROR! 'Projectile Prefabs_Type Sequence' no está asignado o está vacío en el Inspector del Slingshot.");
            enabled = false; return;
        }
        if (anchorPoint == null)
        {
            Debug.LogError("¡ERROR! El Anchor Point no está asignado en el Inspector.");
            enabled = false; return;
        }
        if (anchorPoint.GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("¡ERROR! El 'anchorPoint' DEBE tener un componente Rigidbody (puede ser Kinematic).");
            enabled = false; return;
        }
        if (spawnPoint == null)
        {
            Debug.LogWarning("Spawn Point no asignado. Usando anchorPoint.position como punto de aparición.");
        }
        if (Camera.main != null)
        {
            cameraFollowScript = Camera.main.GetComponent<CameraFollowProjectile>();
            if (cameraFollowScript == null)
            {
                Debug.LogWarning("Main Camera no tiene CameraFollowProjectile script. Agregando uno.");
                cameraFollowScript = Camera.main.gameObject.AddComponent<CameraFollowProjectile>();
            }
        }
        else
        {
            Debug.LogError("¡No se encontró Main Camera en la escena!");
            enabled = false; return;
        }

        projectilesRemaining_TotalLaunches = totalProjectiles;
        PrepareNextProjectile();
    }

    void Update()
    {
        ProcessInputs();
        UpdateBandsVisuals();

        if (currentProjectile == null || currentProjectileRb == null || !currentProjectileRb.isKinematic)
        {
            if (isDragging) isDragging = false;
            return;
        }

        if (primaryInputStartedThisFrame && !isDragging)
        {
            bool canStartDrag = false;
            if (objectCollider != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(currentInputScreenPosition);
                RaycastHit hit;
                if (objectCollider.Raycast(ray, out hit, 200f))
                {
                    canStartDrag = true;
                }
            }
            else
            {
                canStartDrag = true;
            }

            if (canStartDrag)
            {
                isDragging = true;
            }
        }

        if (isDragging && primaryInputIsHeld)
        {
            DragCurrentProjectile(currentInputScreenPosition);
        }

        if (isDragging && !primaryInputIsHeld)
        {
            isDragging = false;
            ReleaseCurrentProjectile();
        }
    }

    void ReleaseCurrentProjectile()
    {
        if (currentProjectileRb == null || currentSpringJoint == null) return;

        GameObject projectileToLaunch = currentProjectile;
        Rigidbody projectileRbToLaunch = currentProjectileRb;

        currentProjectileRb.isKinematic = false;
        Vector3 launchDirection = anchorPoint.position - projectileToLaunch.transform.position;
        float stretchAmount = launchDirection.magnitude;
        Vector3 launchForce = launchDirection.normalized * stretchAmount * launchForceMultiplier;
        currentProjectileRb.AddForce(launchForce);

        Projectile projectileScript = projectileToLaunch.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.NotifyLaunched();
        }
        else
        {
            Debug.LogWarning("El proyectil lanzado no tiene un script 'Projectile.cs'.");
        }

        if (cameraFollowScript != null)
        {
            cameraFollowScript.StartFollowing(projectileToLaunch.transform);
        }


        currentProjectile = null;
        currentProjectileRb = null;
        Destroy(currentSpringJoint);
        currentSpringJoint = null;

        currentProjectileTypeIndex = (currentProjectileTypeIndex + 1) % projectilePrefabs_TypeSequence.Length;

        if (projectilesRemaining_TotalLaunches >= 0)
        {
            Invoke("PrepareNextProjectile", timeToPrepareNext);
        }
        else
        {
            UpdateBandsVisuals();
        }
    }

    // --- CORRUTINA ELIMINADA ---
    // private IEnumerator ActivateGlideAndFollowDelayed(...) { ... }

    // ... (El resto del script: PrepareNextProjectile, DragCurrentProjectile, etc. no cambian)
    void ProcessInputs()
    {
        primaryInputStartedThisFrame = false;

        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            var primaryTouch = Touchscreen.current.primaryTouch;
            currentInputScreenPosition = primaryTouch.position.ReadValue();
            var touchPhase = primaryTouch.phase.ReadValue();

            if (touchPhase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                primaryInputStartedThisFrame = true;
                primaryInputIsHeld = true;
            }
            else if (touchPhase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                primaryInputIsHeld = true;
            }
            else if (touchPhase == UnityEngine.InputSystem.TouchPhase.Ended || touchPhase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                primaryInputIsHeld = false;
            }
        }
        else if (Mouse.current != null)
        {
            currentInputScreenPosition = Mouse.current.position.ReadValue();
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                primaryInputStartedThisFrame = true;
                primaryInputIsHeld = true;
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                primaryInputIsHeld = false;
            }
            else if (Mouse.current.leftButton.isPressed)
            {
                primaryInputIsHeld = true;
            }
        }
        else
        {
            primaryInputIsHeld = false;
        }
    }

    void PrepareNextProjectile()
    {
        if (currentSpringJoint != null) Destroy(currentSpringJoint);

        if (projectilesRemaining_TotalLaunches > 0)
        {
            GameObject prefabToSpawn = projectilePrefabs_TypeSequence[currentProjectileTypeIndex];
            Vector3 spawnPos = (spawnPoint != null) ? spawnPoint.position : anchorPoint.position;
            currentProjectile = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            currentProjectile.name = prefabToSpawn.name + "_Launch_" + (totalProjectiles - projectilesRemaining_TotalLaunches + 1);

            currentProjectileRb = currentProjectile.GetComponent<Rigidbody>();
            if (currentProjectileRb == null)
            {
                Debug.LogError("¡El prefab del proyectil '" + prefabToSpawn.name + "' no tiene Rigidbody! No se puede preparar.", this);
                Destroy(currentProjectile);
                enabled = false;
                return;
            }
            currentProjectileRb.isKinematic = true;

            currentSpringJoint = currentProjectile.AddComponent<SpringJoint>();
            currentSpringJoint.connectedBody = anchorPoint.GetComponent<Rigidbody>();
            currentSpringJoint.spring = 50f; currentSpringJoint.damper = 5f;
            currentSpringJoint.autoConfigureConnectedAnchor = false;
            currentSpringJoint.anchor = Vector3.zero;
            currentSpringJoint.connectedAnchor = Vector3.zero;

            projectilesRemaining_TotalLaunches--;
        }
        else
        {
            currentProjectile = null; currentProjectileRb = null;
            Debug.Log("No quedan más lanzamientos.");
        }
        UpdateBandsVisuals();
    }

    void DragCurrentProjectile(Vector2 screenPosition)
    {
        if (currentProjectile == null) return;
        Vector3 worldInputPos = GetWorldPositionFromScreen(screenPosition);
        Vector3 directionFromAnchor = worldInputPos - anchorPoint.position;

        if (directionFromAnchor.magnitude > maxStretch)
        {
            directionFromAnchor = directionFromAnchor.normalized * maxStretch;
        }
        currentProjectile.transform.position = anchorPoint.position + directionFromAnchor;
    }

    void UpdateBandsVisuals()
    {
        if (bandLeft == null || bandRight == null) return;
        bool showBands = currentProjectile != null && currentSpringJoint != null && currentProjectileRb != null && currentProjectileRb.isKinematic;
        bandLeft.enabled = showBands;
        bandRight.enabled = showBands;
        if (showBands)
        {
            bandLeft.SetPosition(0, anchorPoint.position);
            bandLeft.SetPosition(1, currentProjectile.transform.position);
            bandRight.SetPosition(0, anchorPoint.position);
            bandRight.SetPosition(1, currentProjectile.transform.position);
        }
    }

    Vector3 GetWorldPositionFromScreen(Vector2 screenPos)
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(screenPos);
        Plane gamePlane = new Plane(Vector3.forward, new Vector3(0, 0, anchorPoint.position.z));
        float enterDistance;

        if (gamePlane.Raycast(cameraRay, out enterDistance))
        {
            return cameraRay.GetPoint(enterDistance);
        }
        else
        {
            if (Camera.main.orthographic)
            {
                Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Camera.main.WorldToScreenPoint(anchorPoint.position).z));
                worldPoint.z = anchorPoint.position.z;
                return worldPoint;
            }
            Debug.LogWarning("El rayo del input no intersectó el plano de juego. Usando profundidad de fallback.");
            return cameraRay.GetPoint(Vector3.Distance(Camera.main.transform.position, anchorPoint.position));
        }
    }
}