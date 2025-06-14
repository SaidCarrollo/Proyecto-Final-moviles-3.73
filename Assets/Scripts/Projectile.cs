using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

[System.Serializable]
public struct DamageModifier
{
    public MaterialType targetMaterial;
    [Tooltip("Multiplicador de daño contra este material. 1 = daño normal, 2 = doble daño, 0.5 = mitad de daño.")]
    public float damageMultiplier;
}
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

    [Tooltip("Prefab del objeto que se soltará (ej. Una piedra con su propio Rigidbody).")]
    public GameObject bombPrefab;

    [Tooltip("Fuerza con la que el proyectil principal es impulsado hacia arriba al soltar la bomba.")]
    public float upwardForceOnDrop = 50f;

    [Header("Ciclo de Vida")]
    public float lifeTimeAfterCollision = 5f;

    [Tooltip("La velocidad que tendrá el proyectil al dirigirse al objetivo.")]
    public float homingSpeed = 30f;

    [Tooltip("Capas con las que el rayo puede colisionar para determinar el objetivo. Usa esto para apuntar a bloques o enemigos.")]
    public LayerMask homingTargetLayers;

    protected Rigidbody rb;
    protected bool isLaunched = false;
    protected bool powerActivated = false;
    protected AudioSource audioSource;
    protected ProjectileGlideControl glideControl;
    protected CameraFollowProjectile cameraFollower;
    private Button glideButton;

    private int currentPierces = 0;
    private bool canStartGliding = true;
    private Slingshot slingshotOwner;
    private bool hasNotifiedOwner = false;
    private bool isPendingDespawn = false;
    private ProjectileSpriteManager spriteManager;

    [Tooltip(" Daño y Multiplicadores de daño contra materiales específicos.")]
    public float baseDamage = 25f;
    public DamageModifier[] damageModifiers;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError("El proyectil necesita un Rigidbody.", this);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        glideControl = GetComponent<ProjectileGlideControl>();
        spriteManager = GetComponent<ProjectileSpriteManager>();
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
                // Debug.LogWarning("No se encontr� un GameObject con el nombre 'GlideButton' en la escena.", this);
            }
        }
    }

    public void SetOwner(Slingshot owner)
    {
        this.slingshotOwner = owner;
    }

    public virtual void NotifyLaunched()
    {
        isLaunched = true;
        powerActivated = false;
        currentPierces = 0;
        canStartGliding = true;

        if (glideControl != null && glideButton != null)
        {
            glideButton.gameObject.SetActive(true);
            glideButton.onClick.RemoveAllListeners();
            glideButton.onClick.AddListener(TryActivateGlide);
        }
    }

    public void TryActivateGlide()
    {
        if (isLaunched && glideControl != null && !glideControl.IsGliding && canStartGliding)
        {
            canStartGliding = false;
            glideControl.ActivateGlide(rb.linearVelocity);
            if (glideButton != null)
            {
                glideButton.gameObject.SetActive(false);
            }
        }
    }


    // En Projectile.cs

    protected virtual void Update()
    {
        if (isLaunched && !powerActivated && powerRequiresTap)
        {
            bool tapOccurred = false;
            Vector2 screenPosition = Vector2.zero;

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                tapOccurred = true;
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                tapOccurred = true;
                screenPosition = Mouse.current.position.ReadValue();
            }

            if (tapOccurred)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                ActivatePower(null, screenPosition);
            }
        }
    }
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!isLaunched || isPendingDespawn) return;
        StructureBlock block = collision.gameObject.GetComponent<StructureBlock>();
        if (block != null)
        {
            float damageMultiplier = 1f;

            foreach (var modifier in damageModifiers)
            {
                if (modifier.targetMaterial == block.materialType)
                {
                    damageMultiplier = modifier.damageMultiplier;
                    break;
                }
            }

            float impactVelocity = collision.relativeVelocity.magnitude;
            float finalDamage = baseDamage * damageMultiplier * (impactVelocity / 10f); 

            block.TakeDamage(finalDamage, MaterialType.Totora); 
        }
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            float impactVelocity = collision.relativeVelocity.magnitude;
            float damageToEnemy = baseDamage * (impactVelocity / 5f); 

            enemy.TakeDamage(damageToEnemy);
        }
        if (spriteManager != null)
        {
            spriteManager.SetCrashSprite();
        }
        DeactivateGlideIfNeeded();

        bool shouldStartDespawn = true;

        switch (powerType)
        {
            case ProjectilePowerType.ExplodeOnImpact:
                if (!powerActivated)
                {
                    ActivatePower(collision.contacts[0].point);
                }
                shouldStartDespawn = false;
                break;
            case ProjectilePowerType.PierceThrough:
                HandlePierce(collision.gameObject);
                if (!powerActivated)
                {
                    shouldStartDespawn = false;
                }
                break;
            case ProjectilePowerType.SplitOnTap:
            case ProjectilePowerType.SpeedBoostOnTap:
            case ProjectilePowerType.Normal:

            default:
                break;
        }

        if (shouldStartDespawn)
        {
            isPendingDespawn = true;
            StartCoroutine(StartDespawnTimer());
        }
    }

    public virtual void ActivatePower(Vector3? activationPoint = null, Vector2? screenPosition = null)
    {
        if (powerActivated || !isLaunched) return;
        powerActivated = true;

        DeactivateGlideIfNeeded();

        if (soundOnActivate != null) audioSource.PlayOneShot(soundOnActivate);
        if (effectOnActivatePrefab != null) Instantiate(effectOnActivatePrefab, activationPoint ?? transform.position, Quaternion.identity);

        bool projectileIsDeactivatedByPower = false;

        switch (powerType)
        {
            case ProjectilePowerType.SplitOnTap:
                PerformSplit();
                gameObject.SetActive(false);
                projectileIsDeactivatedByPower = true;
                break;
            case ProjectilePowerType.SpeedBoostOnTap:
                PerformSpeedBoost();
                break;
            case ProjectilePowerType.ExplodeOnImpact:
                PerformExplosion(activationPoint ?? transform.position);
                gameObject.SetActive(false);
                projectileIsDeactivatedByPower = true;
                break;
            case ProjectilePowerType.DropBomb:
                PerformDropBomb();
                break;
            case ProjectilePowerType.Homing:
                PerformHoming(screenPosition);
                break;
        }

        if (projectileIsDeactivatedByPower)
        {
            NotifySlingshotToPrepareNext();
        }
    }

    protected virtual void NotifySlingshotToPrepareNext()
    {
        if (hasNotifiedOwner) return;
        hasNotifiedOwner = true;
        slingshotOwner?.RequestNextProjectile();
    }

    private IEnumerator StartDespawnTimer()
    {
        yield return new WaitForSeconds(lifeTimeAfterCollision);
        if (this != null)
        {

            Despawn();

        }
    }
    public void ResetProjectileState()
    {
        isLaunched = false;
        powerActivated = false;
        isPendingDespawn = false;
        hasNotifiedOwner = false;
        currentPierces = 0;
        canStartGliding = true;

        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        DeactivateGlideIfNeeded();
        if (spriteManager != null)
        {
            spriteManager.SetIdleSprite(); 
        }

        StopAllCoroutines();
    }

    public void Despawn()
    {
        if (!this.enabled || !gameObject.activeSelf) return;
        StopAllCoroutines();
        NotifySlingshotToPrepareNext();
        this.enabled = false;
        gameObject.SetActive(false);
    }

    protected virtual void DeactivateGlideIfNeeded()
    {
        if (canStartGliding)
        {
            canStartGliding = false;
            if (glideButton != null && glideButton.gameObject.activeSelf)
            {
                glideButton.gameObject.SetActive(false);
            }
        }

        if (glideControl != null && glideControl.IsGliding)
        {
            glideControl.DeactivateGlide();
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
                splitRb.linearVelocity = rotation * Vector3.forward * currentVelocity.magnitude * 0.8f;
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
        }
    }

    protected virtual void PerformDropBomb()
    {
        if (bombPrefab == null)
        {
            Debug.LogError("El 'bombPrefab' no está asignado en el proyectil.", this);
            return;
        }

        GameObject bombInstance = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        Collider selfCollider = GetComponent<Collider>();
        Collider bombCollider = bombInstance.GetComponent<Collider>();

        if (selfCollider != null && bombCollider != null)
        {
            Physics.IgnoreCollision(selfCollider, bombCollider);
        }
        else
        {
            Debug.LogWarning("No se pudo ignorar la colisión porque uno de los objetos no tiene Collider.", this);
        }

        if (rb != null)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * upwardForceOnDrop, ForceMode.Impulse);
        }
    }

    protected virtual void PerformHoming(Vector2? screenPosition)
    {
        if (rb == null || !screenPosition.HasValue) return;

        Ray ray = Camera.main.ScreenPointToRay(screenPosition.Value);
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, 200f, homingTargetLayers))
        {

            targetPoint = hit.point;
        }
        else
        {
            Plane plane = new Plane(Vector3.forward, transform.position);
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                targetPoint = ray.GetPoint(distance);
            }
            else
            {
                Debug.LogError("No se pudo determinar el punto objetivo para el poder Homing.");
                return;
            }
        }

        Vector3 direction = (targetPoint - transform.position).normalized;
        rb.linearVelocity = direction * homingSpeed;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}