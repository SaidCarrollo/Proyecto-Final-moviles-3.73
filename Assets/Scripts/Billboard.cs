using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    public float angulo = 0f;

    private Camera mainCamera;
    private Transform parentTransform;

    void Start()
    {
        mainCamera = Camera.main;
        // Guardamos la referencia al transform del padre para mayor eficiencia.
        parentTransform = transform.parent;

        if (parentTransform == null)
        {
            Debug.LogError("Este script requiere que el objeto sea hijo de otro.", this);
        }
    }

    void LateUpdate()
    {
        // Salimos si no hay cámara o si el objeto no tiene un padre.
        if (mainCamera == null || parentTransform == null) return;

        Vector3 direccionAdelante = (transform.position - mainCamera.transform.position).normalized;

        Quaternion rotacionPadreConAngulo = parentTransform.rotation * Quaternion.Euler(0, 0, angulo);
        Vector3 vectorArriba = rotacionPadreConAngulo * Vector3.up;

        Quaternion rotacionFinal = Quaternion.LookRotation(direccionAdelante, vectorArriba);
        transform.rotation = rotacionFinal;
    }
}