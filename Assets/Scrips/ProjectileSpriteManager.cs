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
        // Establecemos el sprite inicial al arrancar.
        SetIdleSprite();
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