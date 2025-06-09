
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }
    private readonly List<Projectile> activeProjectiles = new List<Projectile>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void RegisterProjectile(Projectile projectile)
    {
        if (!activeProjectiles.Contains(projectile))
        {
            activeProjectiles.Add(projectile);
        }
    }

    public void UnregisterProjectile(Projectile projectile)
    {
        if (activeProjectiles.Contains(projectile))
        {
            activeProjectiles.Remove(projectile);
        }
    }

    public void ForceDespawnAllProjectiles()
    {
        List<Projectile> projectilesToDespawn = new List<Projectile>(activeProjectiles);
        foreach (Projectile p in projectilesToDespawn)
        {
            if (p != null)
            {
                p.Despawn();
            }
        }
        activeProjectiles.Clear();
        Debug.Log("Se ha forzado el despawn de todos los proyectiles activos.");
    }
}