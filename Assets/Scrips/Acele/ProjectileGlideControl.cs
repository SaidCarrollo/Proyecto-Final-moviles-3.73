using UnityEngine;

public class ProjectileGlideControl : MonoBehaviour
{
    [Header("Glide Physics Configuration")]
    public float liftCoefficient = 15f;         // Cu�nta sustentaci�n se genera. Ajusta seg�n masa del Rigidbody.
    public float dragCoefficient = 0.5f;        // Resistencia del aire.
    public float angularDragForce = 2f;         // Resistencia a la rotaci�n, ayuda a estabilizar.
    public float tiltToPitchSensitivity = 30f;  // Grados de pitch por unidad de inclinaci�n del aceler�metro.
    public float pitchCorrectionTorque = 5f;    // Fuerza para corregir el pitch hacia el control del jugador.
    public float yawAlignmentTorque = 3f;       // Fuerza para alinear el yaw con la direcci�n de la velocidad.

    [Header("Glide Speed & Angle Limits")]
    public float minSpeedForLift = 3f;        // Velocidad m�nima para una sustentaci�n efectiva.
    public float maxPitchAngle = 60f;         // M�ximo �ngulo de cabeceo hacia arriba.
    public float minPitchAngle = -45f;        // M�ximo �ngulo de cabeceo hacia abajo (picada).

    [Header("Glide Constraints")]
    public float maxYPosition = 30f;
    public float minYPosition = 0.5f;         // Asumiendo que 0 es el suelo.
    public float rotationSmoothness = 5f;     // Para Slerp de rotaci�n general (menos usado ahora).

    private Rigidbody rb;
    private bool isGliding = false;

    public bool IsGliding => isGliding;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("ProjectileGlideControl requires a Rigidbody on the projectile.", this);
            enabled = false;
            return;
        }
        // Aseg�rate de que el Rigidbody tenga una masa y drag razonables por defecto.
        // El drag lineal del Rigidbody se puede poner a 0 si usamos nuestro propio dragForce.
        // rb.drag = 0; // Opcional, si nuestro dragCoefficient es el principal.
        enabled = false;
    }

    public void ActivateGlide(Vector3 launchVelocity)
    {
        if (rb == null) return;

        this.enabled = true;
        isGliding = true;
        rb.useGravity = true; // La gravedad DEBE estar activa.
        rb.linearVelocity = launchVelocity; // Velocidad inicial.

        // Orientaci�n inicial (puede ser refinada)
        if (launchVelocity.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(launchVelocity.normalized); //
        }
        Debug.Log(gameObject.name + " Realistic Glide Control Activated (using forces). Velocity: " + launchVelocity);
    }

    void FixedUpdate()
    {
        if (!isGliding || rb == null) return;

        Vector3 currentVelocity = rb.linearVelocity;
        float speed = currentVelocity.magnitude;
        Vector3 velocityDirection = speed > 0.01f ? currentVelocity.normalized : transform.forward;

        // --- 1. Input del Jugador (Aceler�metro para Pitch) ---
        float tiltInput = -Input.acceleration.y;

        // --- 2. Calcular Fuerzas Aerodin�micas ---
        // Sustentaci�n (Lift)
        float speedFactor = Mathf.Clamp01(speed / minSpeedForLift); // Menos sustentaci�n a baja velocidad.
        // La sustentaci�n es proporcional al cuadrado de la velocidad y al �ngulo de ataque (pitch).
        // Aqu�, el 'pitch' del transform (su orientaci�n local X) influye en transform.up.
        // Un modelo simplificado: la sustentaci�n es mayormente hacia el 'arriba' local del ave.
        float liftPower = liftCoefficient * speed * speedFactor; // o speed * speed
        Vector3 liftForce = transform.up * liftPower;
        rb.AddForce(liftForce);

        // Resistencia (Drag)
        Vector3 dragForce = -velocityDirection * dragCoefficient * speed * speed; //
        rb.AddForce(dragForce);

        // --- 3. Control de Rotaci�n (Torque) ---
        // Pitch Control (Cabeceo basado en input)
        float targetPitch = Mathf.Clamp(tiltInput * tiltToPitchSensitivity, minPitchAngle, maxPitchAngle);
        float currentPitch = GetSignedEulerAngle(transform.eulerAngles.x);

        float pitchError = Mathf.DeltaAngle(currentPitch, targetPitch);
        Vector3 pitchTorque = transform.right * pitchError * pitchCorrectionTorque;
        rb.AddTorque(pitchTorque);

        // Yaw Control (Alinearse con la direcci�n de la velocidad horizontal)
        Vector3 velocityHorizontalDir = new Vector3(velocityDirection.x, 0, velocityDirection.z).normalized;
        if (velocityHorizontalDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetYawRotation = Quaternion.LookRotation(velocityHorizontalDir);
            Quaternion currentYawRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            float yawError = Quaternion.Angle(currentYawRotation, targetYawRotation); // Simplificado

            // Determinar si girar a la izquierda o derecha
            Vector3 localVelDir = transform.InverseTransformDirection(velocityHorizontalDir);
            float yawSign = Mathf.Sign(localVelDir.x);

            Vector3 yawTorque = transform.up * yawError * yawSign * yawAlignmentTorque * -1f; // -1f puede necesitar ajuste
            rb.AddTorque(yawTorque);
        }

        // Estabilizaci�n de Roll (Evitar que d� vueltas sobre s� mismo)
        float currentRoll = GetSignedEulerAngle(transform.eulerAngles.z);
        Vector3 rollTorque = -transform.forward * currentRoll * angularDragForce; // Corregir hacia roll = 0
        rb.AddTorque(rollTorque);


        // Limitar velocidad angular general para que no gire demasiado r�pido
        rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, angularDragForce * 2f); // Ajustar multiplicador


        // --- 4. L�mites de Altitud ---
        Vector3 currentPosition = rb.position;
        if (currentPosition.y > maxYPosition && rb.linearVelocity.y > 0)
        {
            rb.position = new Vector3(currentPosition.x, maxYPosition, currentPosition.z);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }
        else if (currentPosition.y < minYPosition && rb.linearVelocity.y < 0)
        {
            rb.position = new Vector3(currentPosition.x, minYPosition, currentPosition.z);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * -0.3f, rb.linearVelocity.z); // Peque�o rebote o parada
            // Opcionalmente, desactivar planeo si toca el suelo:
            // DeactivateGlide();
            // Projectile script podr�a manejar esto en OnCollisionEnter tambi�n.
��������}
    }

    // Helper para obtener �ngulos entre -180 y 180
    float GetSignedEulerAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;
        return angle;
    }

    public void DeactivateGlide()
    {
        if (!isGliding) return;

        isGliding = false;
        this.enabled = false;
        // La gravedad ya deber�a estar activa.
        // Puedes resetear el drag angular si lo modificaste mucho en el Rigidbody.
        // rb.angularDrag = originalAngularDrag;
        Debug.Log(gameObject.name + " Realistic Glide Control Deactivated (using forces).");
    }
}