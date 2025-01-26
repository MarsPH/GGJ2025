using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance; // Singleton instance

    private Vector3 lastCheckpointPosition; // Store the last checkpoint position

    private void Awake()
    {
        // Ensure only one instance of the checkpoint manager exists
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

    public void SetCheckpoint(Vector3 position)
    {
        lastCheckpointPosition = position;
    }

    public Vector3 GetCheckpointPosition()
    {
        return lastCheckpointPosition;
    }
}
