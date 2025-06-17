using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Configuración de Vida")]
    [Tooltip("La vida inicial del enemigo. Un valor bajo como 5 o 10 hará que sea muy débil.")]
    public float maxHealth = 5f;
    [Header("Daño por Impacto/Caída")]
    [Tooltip("La velocidad mínima de colisión para que el enemigo reciba daño.")]
    public float minDamageVelocity = 3f;
    [Tooltip("Multiplicador para calcular el daño basado en la velocidad del impacto.")]
    public float fallDamageMultiplier = 1.5f;
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
    private void OnCollisionEnter(Collision collision)
    {
        if (currentHealth <= 0) return;

        if (collision.gameObject.GetComponent<Projectile>() != null)
        {
            return;
        }

        float impactVelocity = collision.relativeVelocity.magnitude;

        if (impactVelocity > minDamageVelocity)
        {
            float damage = impactVelocity * fallDamageMultiplier;

            Debug.Log($"{gameObject.name} recibió {damage} de daño por impacto de {collision.gameObject.name} a una velocidad de {impactVelocity}");

            TakeDamage(damage);
        }
    }
}