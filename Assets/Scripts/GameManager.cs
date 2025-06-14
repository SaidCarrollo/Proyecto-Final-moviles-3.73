using UnityEngine;
using System.Collections; // Necesario para la corutina

public class GameManager : SingletonNonPersistent<GameManager>
{
    public static event System.Action OnGameWon;
    public static event System.Action OnGameLost;

    private int enemiesRemaining;
    private bool isGameOver = false;

    void Start()
    {
        enemiesRemaining = FindObjectsOfType<Enemy>().Length;

        if (enemiesRemaining == 0)
        {
            Debug.LogWarning("No se encontraron enemigos en el nivel. Condición de victoria inmediata.");
            WinGame();
        }
    }


    public void RegisterEnemyDeath()
    {
        if (isGameOver) return; 

        enemiesRemaining--;

        if (enemiesRemaining <= 0)
        {
            WinGame();
        }
    }

    public void NotifyOutOfProjectiles()
    {
        if (isGameOver) return;

        StartCoroutine(CheckLoseConditionDelayed());
    }

    private IEnumerator CheckLoseConditionDelayed()
    {
        yield return new WaitForSeconds(3f);

        if (!isGameOver && enemiesRemaining > 0)
        {
            LoseGame();
        }
    }

    private void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("¡GANASTE EL NIVEL!");
        OnGameWon?.Invoke(); 
    }

    private void LoseGame()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("¡PERDISTE! Te quedaste sin aves.");
        OnGameLost?.Invoke(); 
    }
}