using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Guardamos la referencia a la c�mara principal para no tener que buscarla en cada frame.
        mainCamera = Camera.main;
    }

    // Usamos LateUpdate para asegurarnos de que la rotaci�n se aplica DESPU�S
    // de que la c�mara se haya movido en el frame actual.
    void LateUpdate()
    {
        if (mainCamera == null) return;

        // Hacemos que la rotaci�n de este objeto sea la misma que la de la c�mara.
        // Esto mantiene el sprite siempre orientado hacia el jugador, sin inclinarse.
        transform.rotation = mainCamera.transform.rotation;
    }
}