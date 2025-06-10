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

        // --- 1. Lógica de Límites de Altitud y Eje Z ---
        HandleAltitudeAndDepthLimits(velocity);

        // --- 2. Input del Jugador ---
        // El input ahora controla el "ángulo de ataque" (AoA).
        // Tirar hacia arriba (tilt>0) = más sustentación y más arrastre.
        // Picar (tilt<0) = menos sustentación y menos arrastre.
        float tiltInput = -Input.acceleration.y * controlSensitivity;
        float angleOfAttack = Mathf.Clamp(tiltInput, -0.5f, 1.0f); // Limitamos para que no sea super efectivo en picada.

        // --- 3. Cálculo de Fuerzas Aerodinámicas ---
        Vector3 velocityDirection = velocity.normalized;
        float speedSqr = velocity.sqrMagnitude;

        // La dirección de la sustentación es siempre perpendicular a la velocidad.
        Vector3 liftDirection = new Vector3(-velocityDirection.y, velocityDirection.x, 0).normalized;

        // La magnitud de la sustentación depende de la velocidad al cuadrado y el ángulo de ataque.
        float liftCoefficient = angleOfAttack > 0 ? angleOfAttack : 0; // Solo generamos sustentación positiva.
        float liftForceMagnitude = speedSqr * liftCoefficient * liftMultiplier;
        Vector3 liftForce = liftDirection * liftForceMagnitude;

        // La magnitud del arrastre tiene dos partes:
        // 1. Arrastre Parásito: por la forma del objeto y la velocidad.
        float parasiticDragForceMagnitude = speedSqr * parasiticDrag;
        // 2. Arrastre Inducido: por el acto de generar sustentación. ¡Esta es la clave!
        float inducedDragForceMagnitude = liftCoefficient * liftCoefficient * inducedDrag; // Cuadrático para que sea muy notorio.

        Vector3 dragForce = -velocityDirection * (parasiticDragForceMagnitude + inducedDragForceMagnitude);

        // --- 4. Aplicar Fuerzas y Rotación ---
        rb.AddForce(liftForce);
        rb.AddForce(dragForce);

        // La rotación visual sigue a la dirección de la velocidad.
        float visualAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, visualAngle - 90);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.fixedDeltaTime);
    }

    private void HandleAltitudeAndDepthLimits(Vector3 currentVelocity)
    {
        // Corrección del Eje Z
        float zError = targetZPosition - rb.position.z;
        rb.AddForce(0, 0, zError * zCorrectionForce, ForceMode.Force);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z * 0.9f);

        // Límites de Altitud
        if ((rb.position.y >= maxYPosition && currentVelocity.y > 0) || (rb.position.y <= minYPosition && currentVelocity.y < 0))
        {
            // Si tocamos el techo o el suelo, forzamos la velocidad vertical a cero para no quedarnos pegados.
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