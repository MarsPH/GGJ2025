using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public int TotalCollectibles = 5;
    public int MaxHearts = 3;

    [Header("Game State")]
    public int CollectedCount = 0;
    public int CurrentHearts;

    [Header("UI References")]
    public Slider collectiblesSlider; // Reference to a Slider for collectibles
    public TextMeshProUGUI collectiblesText; // Reference to TextMeshPro for the score display
    public Image[] heartImages; // Array for heart UI images
    public Sprite fullHeartSprite; // Sprite for a full heart
    public Sprite emptyHeartSprite; // Sprite for an empty heart

    private bool gameIsOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Persist between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ResetGameState(); // Initialize game state when the game starts
        SetupUI(); // Initialize UI components
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
            SetupUI();
        }
    }

    public void ResetGameState()
    {
        CurrentHearts = MaxHearts;
        CollectedCount = 0;
        gameIsOver = false;

        UpdateHeartsUI();
        UpdateCollectiblesUI();
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
        ResetGameState();
        SceneManager.LoadScene("GameScene");
    }

    public void DecreaseHeart()
    {
        if (CurrentHearts > 0)
        {
            CurrentHearts--;
            Debug.Log($"Hearts Left: {CurrentHearts}");

            UpdateHeartsUI();

            if (CurrentHearts <= 0)
            {
                GameOver();
            }
        }
    }

    public void CollectItem()
    {
        CollectedCount++;
        Debug.Log($"Collected {CollectedCount}/{TotalCollectibles} items.");

        UpdateCollectiblesUI();

        if (CollectedCount >= TotalCollectibles)
        {
            WinGame();
        }
    }

    private void SetupUI()
    {
        // Ensure the slider and text are set up properly
        if (collectiblesSlider != null)
        {
            collectiblesSlider.maxValue = TotalCollectibles; // Set slider max value to the total collectibles
            collectiblesSlider.value = CollectedCount; // Start the slider at the current count
        }

        UpdateHeartsUI(); // Initialize heart display
        UpdateCollectiblesUI(); // Update text and slider
    }

    private void UpdateCollectiblesUI()
    {
        // Update Slider
        if (collectiblesSlider != null)
        {
            collectiblesSlider.value = CollectedCount;
        }

        // Update Text
        if (collectiblesText != null)
        {
            collectiblesText.text = $"Score: {CollectedCount}/{TotalCollectibles}";
        }
    }

    private void UpdateHeartsUI()
    {
        // Update each heart image based on current health
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < CurrentHearts)
            {
                // Set to full heart sprite
                heartImages[i].sprite = fullHeartSprite;
                heartImages[i].color = Color.white; // Ensure it's visible
            }
            else
            {
                // Set to empty heart sprite
                heartImages[i].sprite = emptyHeartSprite;
                heartImages[i].color = new Color(55f / 255f, 87f / 255f, 83f / 255f, 0.5f); // Semi-transparent
            }
        }
    }
}
