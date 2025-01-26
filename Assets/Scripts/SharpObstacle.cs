using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharpObstacle : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object entering the trigger is a bubble
        if (collision.CompareTag("Bubble"))
        {
            // Call the bubble's destruction method
            Blob bubble = collision.GetComponent<Blob>();
            if (bubble != null)
            {
                bubble.DestroyBubble();
            }
        }
    }
}
