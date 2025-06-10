using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; 

public class Projectile : MonoBehaviour
{
    [Header("Configuracion del Poder")]
    public ProjectilePowerType powerType = ProjectilePowerType.Normal; 
    public bool powerRequiresTap = false; 
    public GameObject effectOnActivatePrefab; 
    public AudioClip soundOnActivate; 

    [Header("Parametros Especificos del Poder")]
    public float explosionRadius = 2f; 
    public float explosionForce = 500f; 
    public LayerMask explodableLayers; 
    public GameObject[] splitProjectilePrefabs; 
    public int numberOfSplits = 3; 
    public float splitSpreadAngle = 30f; 
    public float speedBoostMultiplier = 1.5f; 
    public int maxPierces = 1; 
    private int currentPierces = 0; 

    protected Rigidbody rb; 
    protected bool isLaunched = false; 
    protected bool powerActivated = false; 
    protected AudioSource audioSource; 

    [Header("Boton de planeo")]
    protected ProjectileGlideControl glideControl; 
    protected CameraFollowProjectile cameraFollower; 
    private Button glideButton; 
    private bool canStartGliding = true; 

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>(); 
        if (rb == null) Debug.LogError("El proyectil necesita un Rigidbody.", this); 

        audioSource = GetComponent<AudioSource>(); 
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>(); 

        glideControl = GetComponent<ProjectileGlideControl>(); 
        if (Camera.main != null) 
        {
            cameraFollower = Camera.main.GetComponent<CameraFollowProjectile>(); 
        }

        GameObject glideButtonObject = GameObject.Find("GlideButton");
        if (glideButtonObject != null)
        {
            glideButton = glideButtonObject.GetComponent<Button>();
            glideButton.gameObject.SetActive(false);
        }
        else
        {
            if (glideControl != null)
            {
                Debug.LogWarning("No se encontró un GameObject con el nombre 'GlideButton' en la escena.", this);
            }
        }
    }

    public virtual void NotifyLaunched()
    {
        isLaunched = true; 
        powerActivated = false; 
        currentPierces = 0; 

        if (glideControl != null && glideButton != null)
        {
            canStartGliding = true;
            glideButton.gameObject.SetActive(true); 
            glideButton.onClick.RemoveAllListeners(); 
            glideButton.onClick.AddListener(TryActivateGlide); 
        }
    }

    public void TryActivateGlide()
    {
        // Solo activa el planeo si no se ha activado ya y si el control existe
        if (isLaunched && glideControl != null && !glideControl.IsGliding && canStartGliding)
        {
            canStartGliding = false;
            glideControl.ActivateGlide(rb.linearVelocity);
            if (glideButton != null)
            {
                glideButton.gameObject.SetActive(false); // Ocultar el botón después de usarlo
            }
        }
    }

    protected virtual void Update()
    {
        if (isLaunched && !powerActivated && powerRequiresTap) 
        {
            bool tapOccurred = false; 
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began) 
            {
                tapOccurred = true; 
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) 
            {
                tapOccurred = true; 
            }

            if (tapOccurred) 
            {
                ActivatePower(); 
            }
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!isLaunched) return; 

        bool shouldStopGlideAndFollow = false; 

        switch (powerType) 
        {
            case ProjectilePowerType.ExplodeOnImpact: 
                if (!powerActivated) 
                {
                    ActivatePower(collision.contacts[0].point); 
                    shouldStopGlideAndFollow = true; 
                }
                break;
            case ProjectilePowerType.PierceThrough: 
                HandlePierce(collision.gameObject); 
                if (powerActivated) 
                {
                    shouldStopGlideAndFollow = true; 
                }
                break;
            case ProjectilePowerType.SplitOnTap: 
            case ProjectilePowerType.SpeedBoostOnTap: 
            case ProjectilePowerType.Normal: 
            default:
                shouldStopGlideAndFollow = true; 
                break;
        }

        if (shouldStopGlideAndFollow) 
        {
            DeactivateGlideAndFollowIfNeeded(); 
        }
    }

    public virtual void ActivatePower(Vector3? activationPoint = null)
    {
        if (powerActivated || !isLaunched) return; 
        powerActivated = true; 

        DeactivateGlideAndFollowIfNeeded(); 

        if (soundOnActivate != null) audioSource.PlayOneShot(soundOnActivate); 
        if (effectOnActivatePrefab != null) Instantiate(effectOnActivatePrefab, activationPoint ?? transform.position, Quaternion.identity); 

        switch (powerType) 
        {
            case ProjectilePowerType.SplitOnTap: 
                PerformSplit(); 
                Destroy(gameObject); 
                break;
            case ProjectilePowerType.SpeedBoostOnTap: 
                PerformSpeedBoost(); 
                break;
            case ProjectilePowerType.ExplodeOnImpact: 
                PerformExplosion(activationPoint ?? transform.position); 
                Destroy(gameObject); 
                break;
        }
    }

    protected virtual void DeactivateGlideAndFollowIfNeeded()
    {
        // --- MODIFICACIÓN: Ocultar el botón si aún estaba activo ---
        if (canStartGliding && glideButton != null)
        {
            glideButton.gameObject.SetActive(false);
            canStartGliding = false; // Ya no se puede empezar a planear
        }

        if (glideControl != null && glideControl.IsGliding) 
        {
            glideControl.DeactivateGlide(); 
        }
        if (cameraFollower != null && cameraFollower.target == this.transform) 
        {
            cameraFollower.StopFollowing(); 
        }
    }

    protected virtual void PerformExplosion(Vector3 explosionCenter)
    {
        Collider[] colliders = Physics.OverlapSphere(explosionCenter, explosionRadius, explodableLayers); 
        foreach (Collider hit in colliders) 
        {
            Rigidbody hitRb = hit.GetComponent<Rigidbody>(); 
            if (hitRb != null) 
            {
                hitRb.AddExplosionForce(explosionForce, explosionCenter, explosionRadius); 
            }
        }
    }

    protected virtual void PerformSplit()
    {
        if (splitProjectilePrefabs == null || splitProjectilePrefabs.Length == 0) return; 
        for (int i = 0; i < numberOfSplits; i++) 
        {
            if (i >= splitProjectilePrefabs.Length) continue; 
            float angle = (i - (numberOfSplits - 1) / 2.0f) * (splitSpreadAngle / (numberOfSplits > 1 ? numberOfSplits - 1 : 1)); 
            Quaternion rotation; 
            Vector3 currentVelocity = rb != null ? rb.linearVelocity : transform.forward; 
            if (currentVelocity.magnitude < 0.1f) 
            {
                rotation = Quaternion.AngleAxis(angle, transform.up) * transform.rotation; 
            }
            else
            {
                Vector3 spreadAxis = Vector3.Cross(currentVelocity.normalized, Vector3.up); 
                if (spreadAxis.sqrMagnitude < 0.01f) spreadAxis = Vector3.Cross(currentVelocity.normalized, Vector3.right); 
                if (spreadAxis.sqrMagnitude < 0.01f) spreadAxis = Vector3.up; 

                rotation = Quaternion.AngleAxis(angle, spreadAxis.normalized) * Quaternion.LookRotation(currentVelocity.normalized); 
            }

            GameObject splitInstance = Instantiate(splitProjectilePrefabs[i], transform.position, rotation); 
            Projectile splitProjectileScript = splitInstance.GetComponent<Projectile>(); 
            Rigidbody splitRb = splitInstance.GetComponent<Rigidbody>(); 

            if (splitRb != null && rb != null) 
            {
                splitRb.linearVelocity = rotation * Vector3.forward * currentVelocity.magnitude * 0.8f; //
            }
            if (splitProjectileScript != null) 
            {
                splitProjectileScript.NotifyLaunched(); 
            }
        }
    }

    protected virtual void PerformSpeedBoost()
    {
        if (rb != null) 
        {
            rb.AddForce(rb.linearVelocity.normalized * speedBoostMultiplier * rb.mass, ForceMode.Impulse); 
        }
    }

    protected virtual void HandlePierce(GameObject collidedObject)
    {
        if (currentPierces < maxPierces) 
        {
            currentPierces++; 
        }
        else
        {
            powerActivated = true; 
            DeactivateGlideAndFollowIfNeeded(); 
        }
    }
}