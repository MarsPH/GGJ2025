using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public int TotalCollectibles = 5;
    public int MaxHearts = 3;

    [Header("Game State")]
    public int CollectedCount = 0;
    public int CurrentHearts;

    private bool gameIsOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ResetGameState(); // Initialize game state when the game starts
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            ResetGameState();
        }
    }

    public void ResetGameState()
    {
        CurrentHearts = MaxHearts;
        CollectedCount = 0;
        gameIsOver = false;
        Debug.Log("Game state reset.");
    }

    public void GameOver()
    {
        if (gameIsOver) return;

        gameIsOver = true;
        Debug.Log("Game Over!");
        SceneManager.LoadScene("GameOverScene");
    }

    public void WinGame()
    {
        Debug.Log("You Win!");
        SceneManager.LoadScene("WinScene");
    }

    public void LoadMainMenu()
    {
        Debug.Log("Loading Main Menu");
        SceneManager.LoadScene("MainMenuScene");
    }

    public void RetryGame()
    {
        Debug.Log("Retrying Game...");
        SceneManager.LoadScene("GameScene");
    }

    public void DecreaseHeart()
    {
        CurrentHearts--;
        Debug.Log($"Hearts Left: {CurrentHearts}");

        if (CurrentHearts <= 0)
        {
            GameOver();
        }
    }

    public void CollectItem()
    {
        CollectedCount++;
        Debug.Log($"Collected {CollectedCount}/{TotalCollectibles} items.");

        if (CollectedCount >= TotalCollectibles)
        {
            WinGame();
        }
    }
}
