using UnityEngine;

public class ProjectileSpriteManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí el objeto que contiene el SpriteRenderer.")]
    public SpriteRenderer spriteRenderer;

    [Header("Sprites de Estado")]
    [Tooltip("Sprite para el estado inicial o por defecto.")]
    public Sprite idleSprite;

    [Tooltip("Sprite que se mostrará cuando el proyectil esté planeando.")]
    public Sprite glideSprite;

    [Tooltip("Sprite que se mostrará al impactar.")]
    public Sprite crashSprite;
    private Rigidbody rb;
    private Vector3 initialScale;
    void Awake()
    {
        if (spriteRenderer == null)
        {
            // Intenta encontrarlo en el mismo objeto si no fue asignado.
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("ERROR: No se ha asignado ni encontrado un 'Sprite Renderer' en este GameObject.", this);
                this.enabled = false;
                return;
            }
        }
        rb = GetComponentInParent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("ERROR: ProjectileSpriteManager no pudo encontrar un Rigidbody en el objeto padre.", this);
            this.enabled = false;
            return;
        }
        initialScale = transform.localScale;
        SetIdleSprite();
    }
     void FixedUpdate()
    {
        // Solo ajustamos la orientación si el proyectil se está moviendo.
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            float horizontalVelocity = rb.linearVelocity.x;

            // Si la velocidad horizontal es negativa (moviéndose a la izquierda), invertimos la escala en X.
            if (horizontalVelocity < -0.1f)
            {
                transform.localScale = new Vector3(-initialScale.x, initialScale.y, initialScale.z);
            }
            // Si es positiva, usamos la escala normal.
            else if (horizontalVelocity > 0.1f)
            {
                transform.localScale = new Vector3(initialScale.x, initialScale.y, initialScale.z);
            }
        }
    }

    public void SetIdleSprite()
    {
        ApplySprite(idleSprite);
    }

    public void SetGlideSprite()
    {
        ApplySprite(glideSprite);
    }

    public void SetCrashSprite()
    {
        ApplySprite(crashSprite);
    }


    private void ApplySprite(Sprite newSprite)
    {

        if (spriteRenderer == null || newSprite == null) return;

        spriteRenderer.sprite = newSprite;
    }
}