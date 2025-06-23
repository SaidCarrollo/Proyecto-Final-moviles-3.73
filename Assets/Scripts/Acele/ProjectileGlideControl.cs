using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProjectileGlideControl : MonoBehaviour
{
    [Header("Energy & Glide Physics")]
    public float liftMultiplier = 0.5f;
    public float parasiticDrag = 0.1f;
    public float inducedDrag = 1.5f;

    [Header("Control")]
    public float controlSensitivity = 1.0f;
    public float diveThrust = 2.0f;

    [Header("Dive Recovery")]
    public float diveRecoveryBonus = 1.5f;

    [Header("2.5D Constraints")]
    public float targetZPosition = 0f;
    public float zCorrectionForce = 10f;

    [Header("Glide Altitude Limits")]
    public float maxYPosition = 30f;
    public float minYPosition = 0.5f;

    [Header("Visual Rotation")]
    public float rotationSmoothness = 5f;

    [Header("Sonidos")]
    public AudioClip onGlideActivateSound;

    private Rigidbody rb;
    private bool isGliding = false;
    private ProjectileSpriteManager spriteManager;
    private float previousYVelocity = 0f;
    public bool IsGliding => isGliding;
    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spriteManager = GetComponent<ProjectileSpriteManager>();
        audioSource = GetComponent<AudioSource>();

        if (rb == null)
        {
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

        if (audioSource != null && onGlideActivateSound != null)
        {
            audioSource.PlayOneShot(onGlideActivateSound);
        }

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
    }
}