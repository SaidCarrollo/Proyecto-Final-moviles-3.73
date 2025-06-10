using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Guardamos la referencia a la cámara principal para no tener que buscarla en cada frame.
        mainCamera = Camera.main;
    }

    // Usamos LateUpdate para asegurarnos de que la rotación se aplica DESPUÉS
    // de que la cámara se haya movido en el frame actual.
    void LateUpdate()
    {
        if (mainCamera == null) return;

        // Hacemos que la rotación de este objeto sea la misma que la de la cámara.
        // Esto mantiene el sprite siempre orientado hacia el jugador, sin inclinarse.
        transform.rotation = mainCamera.transform.rotation;
    }
}