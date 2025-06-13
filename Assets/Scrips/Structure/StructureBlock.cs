
using UnityEngine;
public enum MaterialType
{
    Totora, 
    Adobe,  
    Piedra  
}
public class StructureBlock : MonoBehaviour
{
    [Header("Configuraci�n del Material")]
    public MaterialType materialType;
    [Tooltip("La cantidad de vida inicial del bloque. Depender� del material.")]
    public float maxHealth = 100f;

    [Header("Efectos Visuales y de Sonido (Opcional)")]
    public GameObject destructionEffectPrefab;
    public AudioClip impactSound;
    public AudioClip destructionSound;

    private float currentHealth;
    private AudioSource audioSource;

    void Awake()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void TakeDamage(float damageAmount, MaterialType damageSourceType)
    {

        currentHealth -= damageAmount;

        if (audioSource != null && impactSound != null)
        {
            audioSource.PlayOneShot(impactSound);
        }

        if (currentHealth <= 0)
        {
            DestroyBlock();
        }
        else
        {
            // Opcional: Actualizar el sprite o material para mostrar da�o
            UpdateVisuals();
        }
    }

    private void DestroyBlock()
    {
        if (destructionEffectPrefab != null)
        {
            Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
        }

        if (audioSource != null && destructionSound != null)
        {
            // Para que el sonido de destrucci�n se reproduzca antes de desactivar el objeto
            AudioSource.PlayClipAtPoint(destructionSound, transform.position);
        }

        // Desactivamos el objeto. Usar Destroy() tambi�n es una opci�n.
        gameObject.SetActive(false);
    }

    private void UpdateVisuals()
    {

    }
}
