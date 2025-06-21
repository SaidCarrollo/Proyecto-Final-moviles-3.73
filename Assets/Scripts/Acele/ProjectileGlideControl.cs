using UnityEngine;

public class ProjectileGlideControl : MonoBehaviour
{
    [Header("Energy & Glide Physics")]
    [Tooltip("Multiplicador principal de la sustentación. Qué tan 'flotador' es el proyectil.")]
    public float liftMultiplier = 0.5f;
    [Tooltip("Arrastre base del proyectil, simplemente por moverse en el aire.")]
    public float parasiticDrag = 0.1f;
    [Tooltip("Arrastre EXTRA que se genera al 'tirar hacia arriba' para generar sustentación. Clave para el realismo.")]
    public float inducedDrag = 1.5f;

    [Header("Control")]
    [Tooltip("Sensibilidad del acelerómetro para controlar el ángulo de ataque.")]
    public float controlSensitivity = 1.0f;
    [Tooltip("Fuerza de empuje adicional al picar para ganar velocidad. 0 para desactivar.")]
    public float diveThrust = 2.0f;

    [Header("Dive Recovery")]
    [Tooltip("Bono de sustentación al recuperarse de una picada. >1 para un efecto notable.")]
    public float diveRecoveryBonus = 1.5f;

    [Header("2.5D Constraints")]
    public float targetZPosition = 0f;
    public float zCorrectionForce = 10f;

    [Header("Glide Altitude Limits")]
    public float maxYPosition = 30f;
    public float minYPosition = 0.5f;

    [Header("Visual Rotation")]
    public float rotationSmoothness = 5f;

    private Rigidbody rb;
    private bool isGliding = false;
    private ProjectileSpriteManager spriteManager;
    private float previousYVelocity = 0f;
    public bool IsGliding => isGliding;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spriteManager = GetComponent<ProjectileSpriteManager>();
        if (rb == null)
        {
            Debug.LogError("ProjectileGlideControl requiere un Rigidbody.", this);
            enabled = false;
            return;
        }
        enabled = false;
    }

    public void ActivateGlide(Vector3 launchVelocity)
    {
        if (rb == null) return;
        this.enabled = true;
        isGliding = true;
        rb.useGravity = true;
        rb.linearVelocity = launchVelocity;
        targetZPosition = transform.position.z;
        previousYVelocity = rb.linearVelocity.y;
        Debug.Log(gameObject.name + " ENERGY GLIDE Activated.");
        if (spriteManager != null)
        {
            spriteManager.SetGlideSprite();
        }
    }

    void FixedUpdate()
    {
        if (!isGliding || rb == null) return;

        Vector3 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude < 0.1f) return;

        HandleAltitudeAndDepthLimits(velocity);

        float tiltInput = -Input.acceleration.y * controlSensitivity;
        float angleOfAttack = Mathf.Clamp(tiltInput, -0.5f, 1.0f);

        Vector3 velocityDirection = velocity.normalized;
        float speedSqr = velocity.sqrMagnitude;

        Vector3 liftDirection = new Vector3(-velocityDirection.y, velocityDirection.x, 0).normalized;
        float liftCoefficient = angleOfAttack > 0 ? angleOfAttack : 0;
        float liftForceMagnitude = speedSqr * liftCoefficient * liftMultiplier;

        if (previousYVelocity < 0 && tiltInput > 0)
        {
            liftForceMagnitude *= diveRecoveryBonus;
        }
        Vector3 liftForce = liftDirection * liftForceMagnitude;

        float parasiticDragForceMagnitude = speedSqr * parasiticDrag;
        float inducedDragForceMagnitude = liftCoefficient * liftCoefficient * inducedDrag;
        Vector3 dragForce = -velocityDirection * (parasiticDragForceMagnitude + inducedDragForceMagnitude);

        Vector3 thrustForce = Vector3.zero;
        if (tiltInput < 0)
        {
            thrustForce = velocityDirection * Mathf.Abs(tiltInput) * diveThrust;
        }

        rb.AddForce(liftForce);
        rb.AddForce(dragForce);
        rb.AddForce(thrustForce);

        float visualAngle = Mathf.Atan2(velocity.y, Mathf.Abs(velocity.x)) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, visualAngle - 90);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.fixedDeltaTime);

        previousYVelocity = velocity.y;
    }

    private void HandleAltitudeAndDepthLimits(Vector3 currentVelocity)
    {
        float zError = targetZPosition - rb.position.z;
        rb.AddForce(0, 0, zError * zCorrectionForce, ForceMode.Force);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z * 0.9f);

        if ((rb.position.y >= maxYPosition && currentVelocity.y > 0) || (rb.position.y <= minYPosition && currentVelocity.y < 0))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }
    }

    public void DeactivateGlide()
    {
        if (!isGliding) return;
        isGliding = false;
        this.enabled = false;
        Debug.Log(gameObject.name + " ENERGY GLIDE Deactivated.");
    }
}