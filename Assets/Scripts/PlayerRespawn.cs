using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public float respawnDelay = 1f; // Delay before respawning
    public GameObject bubblePrefab; // Reference to the bubble prefab
    private GameObject currentBlob;
    private Blob blob;

    private void Start()
    {
        currentBlob = Resources.Load<GameObject>("BubblePrefab");
        blob = currentBlob.GetComponent<Blob>();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void Respawn()
    {
        Debug.Log("Respawn");
        // Get the checkpoint position
        Vector3 respawnPosition = CheckpointManager.Instance.GetCheckpointPosition();
        Debug.Log($"Respawning at {respawnPosition}");
        if (respawnPosition != Vector3.zero && currentBlob != null)
        {
            // Use the SpawnNewBubble method from Blob
            blob.SpawnNewBubble(respawnPosition, blob.GetBubbleSize());
            Debug.Log("Respawned bubble at checkpoint!");
        }
        else
        {
            Debug.LogWarning("No checkpoint found or Blob script is missing!");
        }
    }

    public IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        // Get the checkpoint position
        Vector3 respawnPosition = CheckpointManager.Instance.GetCheckpointPosition();
        Debug.Log($"Respawning at {respawnPosition}");
        if (respawnPosition != Vector3.zero && currentBlob != null)
        {
            // Use the SpawnNewBubble method from Blob
            blob.SpawnNewBubble(respawnPosition, blob.GetBubbleSize());
            Debug.Log("Respawned bubble at checkpoint!");
        }
        else
        {
            Debug.LogWarning("No checkpoint found or Blob script is missing!");
        }
    }
    
}
