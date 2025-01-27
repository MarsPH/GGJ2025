using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;
    public CanvasGroup canvasGroup; // Assign in Inspector
    public float messageDuration = 3f;

    private string[] tutorialMessages = new string[]
    {
        "Hold Left Click to Push the Bubble.",
        "Be careful with the bubble, don't move it too quickly or stretch it, otherwise it will pop.",
        "Good luck and have fun!"
    };

    private int currentMessageIndex = 0;

    private void Start()
    {
        StartCoroutine(ShowTutorialMessages());
    }

    private System.Collections.IEnumerator ShowTutorialMessages()
    {
        while (currentMessageIndex < tutorialMessages.Length)
        {
            tutorialText.text = tutorialMessages[currentMessageIndex];
            yield return StartCoroutine(FadeIn());

            yield return new WaitForSeconds(messageDuration);

            yield return StartCoroutine(FadeOut());
            currentMessageIndex++;
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private System.Collections.IEnumerator FadeOut()
    {
        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}