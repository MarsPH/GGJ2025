using UnityEngine;
using System.Collections;

public class BubbleSpawner : MonoBehaviour
{
    [Header("Bubble Settings")]
    public GameObject bubblePrefab; // Assign your bubble prefab in the Inspector
    public int numberOfBubbles = 10; // Total number of bubbles to spawn
    public Vector2 spawnAreaMin; // Bottom-left corner of the spawn area
    public Vector2 spawnAreaMax; // Top-right corner of the spawn area
    public Vector2 sizeRange = new Vector2(0.5f, 2f); // Min and max size for bubbles
    public Color[] possibleColors; // Array of possible colors for bubbles
    public float spawnInterval = 1.0f; // Time delay between spawning each bubble
    public float bubbleLifespan = 5.0f; // Time before a bubble is destroyed

    private void Start()
    {
        StartCoroutine(SpawnBubbles());
    }

    private IEnumerator SpawnBubbles()
    {
        for (int i = 0; i < numberOfBubbles; i++)
        {
            // Random position within the spawn area
            Vector2 spawnPosition = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );

            // Spawn the bubble
            GameObject newBubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);

            // Randomize size
            float randomSize = Random.Range(sizeRange.x, sizeRange.y);
            newBubble.transform.localScale = new Vector3(randomSize, randomSize, 1);

            // Randomize color (modifying the material color)
            if (possibleColors.Length > 0)
            {
                Color randomColor = possibleColors[Random.Range(0, possibleColors.Length)];
                Renderer renderer = newBubble.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Modify the "_BaseColor" property in the material shader
                    //renderer.material.SetColor("_Color", randomColor);
                }
            }

            // Optionally make the bubbles children of the spawner for organization
            newBubble.transform.SetParent(transform);

            // Destroy the bubble after its lifespan
            Destroy(newBubble, bubbleLifespan);

            // Wait before spawning the next bubble
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
