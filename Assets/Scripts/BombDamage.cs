using UnityEngine;

public class BombDamage : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [Tooltip("La cantidad de daño base que inflige la bomba al impactar.")]
    public float damageAmount = 50f;

    [Tooltip("El radio de la explosión. Si es mayor a 0, dañará en área.")]
    public float explosionRadius = 0f;

    [Tooltip("La fuerza de la explosión que se aplicará a los objetos cercanos.")]
    public float explosionForce = 300f;

    [Tooltip("Capas de objetos que pueden ser afectadas por la explosión.")]
    public LayerMask explodableLayers;

    [Header("Efectos y Sonido")]
    [Tooltip("Efecto de partículas que se instancia al explotar.")]
    public GameObject explosionEffectPrefab;

    [Tooltip("Sonido que se reproduce en la explosión.")]
    public AudioClip explosionSound;

    private bool hasExploded = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        hasExploded = true;

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        if (explosionRadius > 0f)
        {
            Explode();
        }
        else
        {

            ApplyDirectDamage(collision.gameObject);
        }

        gameObject.SetActive(false);
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, explodableLayers);

        foreach (Collider hit in colliders)
        {
            ApplyDirectDamage(hit.gameObject);

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
    }

    private void ApplyDirectDamage(GameObject target)
    {
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damageAmount);
        }

        StructureBlock block = target.GetComponent<StructureBlock>();
        if (block != null)
        {

            block.TakeDamage(damageAmount, MaterialType.Piedra);
        }
    }
}