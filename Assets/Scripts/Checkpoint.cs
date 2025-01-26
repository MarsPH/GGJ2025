using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerMain"))
        {
            // Save this checkpoint as the player's last checkpoint
            CheckpointManager.Instance.SetCheckpoint(transform.position);
            //Debug.Log("Checkpoint updated!");
        }
    }
}
