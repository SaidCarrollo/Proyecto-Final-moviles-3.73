using UnityEngine;
using System.Collections.Generic;

public class TrajectoryPredictor : MonoBehaviour
{
    [Header("Configuración de la Trayectoria")]
    [Tooltip("El prefab del punto que se usará para dibujar la trayectoria.")]
    public GameObject dotPrefab;

    [Tooltip("Cuántos puntos se usarán para dibujar la línea.")]
    public int dotCount = 15;

    [Tooltip("El espaciado de tiempo entre los puntos de la trayectoria.")]
    public float dotSpacing = 0.08f;

    private List<GameObject> trajectoryDots = new List<GameObject>();

    void Start()
    {
        InitializeTrajectoryDots();
    }

    private void InitializeTrajectoryDots()
    {
        if (dotPrefab == null)
        {
            Debug.LogError("¡Trajectory Dot Prefab no está asignado en el TrajectoryPredictor!");
            enabled = false;
            return;
        }

        for (int i = 0; i < dotCount; i++)
        {
            GameObject dot = Instantiate(dotPrefab, transform);
            trajectoryDots.Add(dot);
        }
        Hide(); 
    }

    public void UpdateTrajectory(Vector3 startPosition, Vector3 initialVelocity)
    {
        if (trajectoryDots.Count == 0) return;

        for (int i = 0; i < dotCount; i++)
        {
            if (!trajectoryDots[i].activeSelf)
            {
                trajectoryDots[i].SetActive(true);
            }

            float t = i * dotSpacing;

            Vector3 position = startPosition + initialVelocity * t + 0.5f * Physics.gravity * t * t;

            trajectoryDots[i].transform.position = position;
        }
    }

    public void Hide()
    {
        if (trajectoryDots.Count == 0) return;
        foreach (var dot in trajectoryDots)
        {
            dot.SetActive(false);
        }
    }
}