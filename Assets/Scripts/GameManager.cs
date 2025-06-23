using UnityEngine;
using System.Collections; 

public class GameManager : SingletonNonPersistent<GameManager>
{
    public static event System.Action OnGameWon;
    public static event System.Action OnGameLost;

    private int enemiesRemaining;
    private bool isGameOver = false;
    public GameObject PanelGameover;
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
        Slingshot slingshot = FindObjectOfType<Slingshot>();
        if (slingshot != null)
        {
            int projectilesSpared = slingshot.projectilesRemaining_TotalLaunches;
            int stars = 1; 

            if (projectilesSpared >= 2)
            {
                stars = 3;
            }
            else if (projectilesSpared == 1)
            {
                stars = 2;
            }

            Debug.Log($"Proyectiles de sobra: {projectilesSpared}. Estrellas obtenidas: {stars} / 3");
        }
        else
        {
            Debug.LogWarning("No se pudo encontrar el Slingshot en la escena para calcular las estrellas.");
        }
        OnGameWon?.Invoke(); 
    }

    private void LoseGame()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("¡PERDISTE! Te quedaste sin aves.");
        OnGameLost?.Invoke();
        PanelGameover.SetActive(true);
    }
}