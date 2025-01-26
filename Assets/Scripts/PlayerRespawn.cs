using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public float respawnDelay = 1f; // Delay before respawning
    public GameObject bubblePrefab; // Reference to the bubble prefab
    private Blob blob;

    private void Start()
    {
        if (bubblePrefab != null)
        {
            blob = bubblePrefab.GetComponent<Blob>();
        }
        else
        {
            Debug.LogError("BubblePrefab is not assigned in PlayerRespawn!");
        }
    }

    public void Respawn()
    {
        Debug.Log("Respawn");

        // Get the checkpoint position
        Vector3 respawnPosition = CheckpointManager.Instance.GetCheckpointPosition();
        Debug.Log($"Respawning at {respawnPosition}");

        if (respawnPosition != Vector3.zero && bubblePrefab != null)
        {
            // Spawn the new bubble at the checkpoint position
            GameObject newBubble = Instantiate(bubblePrefab, respawnPosition, Quaternion.identity);

            // Ensure the new bubble is tagged as "PlayerMain"
            newBubble.tag = "PlayerMain";

            // Update the camera to follow the new bubble
            SmoothCameraFollow cameraFollow = Camera.main.GetComponent<SmoothCameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.UpdateTarget(newBubble.transform);
            }

            Debug.Log("Respawned bubble at checkpoint!");
        }
        else
        {
            Debug.LogWarning("No checkpoint found or BubblePrefab is missing!");
        }
    }
}
