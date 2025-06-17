using UnityEngine;

public class BombDamage : MonoBehaviour
{
    [Header("Configuraci�n de Da�o")]
    [Tooltip("La cantidad de da�o base que inflige la bomba al impactar.")]
    public float damageAmount = 50f;

    [Tooltip("El radio de la explosi�n. Si es mayor a 0, da�ar� en �rea.")]
    public float explosionRadius = 0f;

    [Tooltip("La fuerza de la explosi�n que se aplicar� a los objetos cercanos.")]
    public float explosionForce = 300f;

    [Tooltip("Capas de objetos que pueden ser afectadas por la explosi�n.")]
    public LayerMask explodableLayers;

    [Header("Efectos y Sonido")]
    [Tooltip("Efecto de part�culas que se instancia al explotar.")]
    public GameObject explosionEffectPrefab;

    [Tooltip("Sonido que se reproduce en la explosi�n.")]
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