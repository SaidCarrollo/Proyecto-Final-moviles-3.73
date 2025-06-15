using UnityEngine;
using UnityEngine.UI;
public class LevelSelectorManager : MonoBehaviour
{
// CAMBIO 1: Ahora el array es de tipo 'Image' en lugar de 'GameObject'.
    // Esto es más eficiente y seguro, ya que nos aseguramos de que cada elemento tiene un componente Image.
    [Tooltip("Arrastra aquí los componentes 'Image' de los paneles que bloquean cada nivel.")]
    public Image[] levelBlockers; 

    // CAMBIO 2: Añadimos un color público para definir cómo se ven los niveles bloqueados.
    [Tooltip("Elige el color y la transparencia que tendrán los niveles bloqueados.")]
    public Color lockedColor = new Color(0f, 0f, 0f, 0.5f); // Negro semitransparente por defecto

    // Esta función, como antes, es llamada por el evento OnLoginSuccess del LoginManager
    public void UnlockLevels(int highestLevelUnlocked)
    {
        Debug.Log($"Actualizando UI de niveles. Nivel más alto alcanzado: {highestLevelUnlocked+1}");

        // El bucle sigue la misma lógica
        for (int i = 0; i < levelBlockers.Length; i++)
        {
            Image blockerImage = levelBlockers[i]; // Obtenemos la imagen del panel
            int levelToCheck = i; // El nivel que este panel está bloqueando

            if (levelToCheck <= highestLevelUnlocked)
            {
                // --- Nivel DESBLOQUEADO ---

                // 1. Hacemos que NO bloquee los clics/taps del mouse o dedo.
                blockerImage.raycastTarget = false;

                // 2. Lo hacemos completamente transparente y de color blanco.
                // La transparencia se controla con el cuarto valor (alpha).
                blockerImage.color = new Color(1f, 1f, 1f, 0f); 
            }
            else
            {
                // --- Nivel BLOQUEADO ---

                // 1. Hacemos que SÍ bloquee los clics/taps.
                blockerImage.raycastTarget = true;

                // 2. Le ponemos el color de bloqueo que definimos en el Inspector.
                blockerImage.color = lockedColor;
            }
        }
    }
}