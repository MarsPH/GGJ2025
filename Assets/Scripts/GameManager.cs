using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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

    public void GameOver()
    {
        if (gameIsOver) return;

        gameIsOver = true;
        Debug.Log("Game Over!");
        // Show Game Over UI or transition
        Invoke("RestartGame", 2f); // Restart after 2 seconds
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Restart current scene
    }

    public void WinGame()
    {
        Debug.Log("You Win!");
        SceneManager.LoadScene("WinScene"); // Replace with your actual win scene name
    }
}