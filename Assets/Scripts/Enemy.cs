using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Configuraci�n de Vida")]
    [Tooltip("La vida inicial del enemigo. Un valor bajo como 5 o 10 har� que sea muy d�bil.")]
    public float maxHealth = 5f;
    [Header("Da�o por Impacto/Ca�da")]
    [Tooltip("La velocidad m�nima de colisi�n para que el enemigo reciba da�o.")]
    public float minDamageVelocity = 3f;
    [Tooltip("Multiplicador para calcular el da�o basado en la velocidad del impacto.")]
    public float fallDamageMultiplier = 1.5f;
    [Header("Efectos y Sonidos")]
    [Tooltip("Efecto de part�culas que aparece cuando el enemigo es destruido.")]
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

            Debug.Log($"{gameObject.name} recibi� {damage} de da�o por impacto de {collision.gameObject.name} a una velocidad de {impactVelocity}");

            TakeDamage(damage);
        }
    }
}