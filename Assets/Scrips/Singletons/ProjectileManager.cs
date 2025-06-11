
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : SingletonNonPersistent<ProjectileManager>
{
    private readonly List<Projectile> activeProjectiles = new List<Projectile>();

    private void Awake()
    {
        Time.timeScale = 1;
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