using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Configuración de Vida")]
    [Tooltip("La vida inicial del enemigo. Un valor bajo como 5 o 10 hará que sea muy débil.")]
    public float maxHealth = 5f;

    [Header("Efectos y Sonidos")]
    [Tooltip("Efecto de partículas que aparece cuando el enemigo es destruido.")]
    public GameObject destructionEffectPrefab;
    [Tooltip("Sonido que se reproduce al recibir un impacto.")]
    public AudioClip impactSound;
    [Tooltip("Sonido que se reproduce al ser destruido.")]
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

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

        if (audioSource != null && impactSound != null)
        {
            audioSource.PlayOneShot(impactSound);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.Instance.RegisterEnemyDeath();
        if (destructionEffectPrefab != null)
        {
            Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
        }

        if (destructionSound != null)
        {
            AudioSource.PlayClipAtPoint(destructionSound, transform.position);
        }


        gameObject.SetActive(false);
    }
}