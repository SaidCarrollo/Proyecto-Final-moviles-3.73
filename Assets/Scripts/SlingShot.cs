using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    [Header("Referencias de UI")]
    public Button glideButton;
    public Button despawnButton;
    [Tooltip("Sonido que se reproduce al empezar a arrastrar el proyectil.")]
    public AudioClip onDragStartSound;
    [Tooltip("Sonido que se reproduce continuamente mientras se arrastra.")]
    public AudioClip onDraggingSound;
    [Tooltip("Sonido que se reproduce al lanzar el proyectil.")]
    public AudioClip onLaunchSound;
    public bool isDragging { get; private set; } = false;
    private bool primaryInputStartedThisFrame = false;
    private bool primaryInputIsHeld = false;
    private Vector2 currentInputScreenPosition;

    private GameObject currentProjectile;
    private Rigidbody currentProjectileRb;
    private Projectile currentLaunchedProjectile;
    private int projectilesRemaining_TotalLaunches;

    private Collider objectCollider;
    private CameraFollowProjectile cameraFollowScript;
    [Tooltip("Referencia al script que visualiza la trayectoria.")]
    public TrajectoryPredictor trajectoryPredictor;

    [Tooltip("Multiplicador para ajustar la predicción de la trayectoria si no coincide con el lanzamiento real.")]
    public float trajectoryPredictionMultiplier = 1.0f;

    private AudioSource audioSource;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; 
    }
    void Start()
    {
        if (glideButton != null) glideButton.gameObject.SetActive(false);
        if (despawnButton != null) despawnButton.gameObject.SetActive(false);

        objectCollider = GetComponent<Collider>();
        if (objectCollider == null)
        {
            Debug.LogWarning("Slingshot GameObject no tiene un Collider. Esto ya no afecta el inicio del arrastre.");
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
            cameraFollowScript.slingshotTransform = this.transform;
            cameraFollowScript.ResetToSlingshotView();
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
            if (isDragging)
            {
                isDragging = false;
                if (trajectoryPredictor != null) trajectoryPredictor.Hide();
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
            return;
        }

        if (primaryInputStartedThisFrame && !isDragging && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(currentInputScreenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                Collider projectileCollider = currentProjectile.GetComponent<Collider>();
                if (hit.collider == objectCollider || (projectileCollider != null && hit.collider == projectileCollider))
                {
                    isDragging = true;

                    if (onDraggingSound != null)
                    {
                        audioSource.clip = onDraggingSound;
                        audioSource.loop = true;
                        audioSource.Play();
                    }

                    if (onDragStartSound != null)
                    {
                        audioSource.PlayOneShot(onDragStartSound);
                    }
                }
            }
        }

        if (isDragging && primaryInputIsHeld)
        {
            DragCurrentProjectile(currentInputScreenPosition);
            if (trajectoryPredictor != null)
            {
                Vector3 launchDirection = anchorPoint.position - currentProjectile.transform.position;
                float stretchAmount = launchDirection.magnitude;
                Vector3 launchForce = launchDirection.normalized * stretchAmount * launchForceMultiplier;
                Vector3 initialVelocity = (launchForce / currentProjectileRb.mass) * Time.fixedDeltaTime * trajectoryPredictionMultiplier;
                trajectoryPredictor.UpdateTrajectory(currentProjectile.transform.position, initialVelocity);
            }
        }

        if (isDragging && !primaryInputIsHeld)
        {
            isDragging = false;
            if (trajectoryPredictor != null) trajectoryPredictor.Hide();

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            ReleaseCurrentProjectile();
        }
    }

    void ReleaseCurrentProjectile()
    {
        if (currentProjectileRb == null) return;

        if (onLaunchSound != null)
        {
            audioSource.PlayOneShot(onLaunchSound);
        }

        GameObject projectileToLaunch = currentProjectile;
        Rigidbody projectileRbToLaunch = currentProjectileRb;

        projectileRbToLaunch.isKinematic = false;
        Vector3 launchDirection = anchorPoint.position - projectileToLaunch.transform.position;
        float stretchAmount = launchDirection.magnitude;
        Vector3 launchForce = launchDirection.normalized * stretchAmount * launchForceMultiplier;
        projectileRbToLaunch.AddForce(launchForce);

        Projectile projectileScript = projectileToLaunch.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.NotifyLaunched();
            projectileScript.SetOwner(this);
            currentLaunchedProjectile = projectileScript;

            ProjectileGlideControl glideControl = projectileToLaunch.GetComponent<ProjectileGlideControl>();
            if (glideControl != null && glideButton != null)
            {
                glideButton.gameObject.SetActive(true);
                glideButton.onClick.RemoveAllListeners();
                glideButton.onClick.AddListener(projectileScript.TryActivateGlide);
            }

            if (despawnButton != null)
            {
                despawnButton.gameObject.SetActive(true);
            }
        }

        if (cameraFollowScript != null)
        {
            cameraFollowScript.StartFollowing(projectileToLaunch.transform);
        }

        currentProjectile = null;
        currentProjectileRb = null;

        currentProjectileTypeIndex = (currentProjectileTypeIndex + 1) % projectilePrefabs_TypeSequence.Length;
        if (trajectoryPredictor != null) trajectoryPredictor.Hide();
    }

    public void RequestNextProjectile()
    {
        HideGlideButton();
        if (despawnButton != null) despawnButton.gameObject.SetActive(false);
        currentLaunchedProjectile = null;

        if (cameraFollowScript != null)
        {
            cameraFollowScript.ResetToSlingshotView();
        }

        if (projectilesRemaining_TotalLaunches > 0)
        {
            Invoke("PrepareNextProjectile", timeToPrepareNext);
        }
        else
        {
            GameManager.Instance.NotifyOutOfProjectiles();
            UpdateBandsVisuals();
        }
    }

    public void HideGlideButton()
    {
        if (glideButton != null) glideButton.gameObject.SetActive(false);
    }

    public void DespawnCurrentProjectile()
    {
        if (currentLaunchedProjectile != null)
        {
            currentLaunchedProjectile.Despawn();
            currentLaunchedProjectile = null;
        }
        if (despawnButton != null) despawnButton.gameObject.SetActive(false);
    }

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
        if (projectilesRemaining_TotalLaunches > 0)
        {
            GameObject prefabToSpawn = projectilePrefabs_TypeSequence[currentProjectileTypeIndex];
            Vector3 spawnPos = (spawnPoint != null) ? spawnPoint.position : anchorPoint.position;

            currentProjectile = ObjectPooler.Instance.SpawnFromPool(prefabToSpawn.name, spawnPos, Quaternion.identity);

            if (currentProjectile == null)
            {
                return;
            }
            currentProjectile.name = prefabToSpawn.name + "_Launch_" + (totalProjectiles - projectilesRemaining_TotalLaunches + 1);

            currentProjectileRb = currentProjectile.GetComponent<Rigidbody>();
            if (currentProjectileRb == null)
            {
                currentProjectile.SetActive(false);
                enabled = false;
                return;
            }

            currentProjectileRb.isKinematic = true;
            projectilesRemaining_TotalLaunches--;
        }
        else
        {
            currentProjectile = null;
            currentProjectileRb = null;
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

        bool showBands = currentProjectile != null && currentProjectileRb != null && currentProjectileRb.isKinematic;

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
            return cameraRay.GetPoint(Vector3.Distance(Camera.main.transform.position, anchorPoint.position));
        }
    }
}