using UnityEngine;

public class Splitter : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.CompareTag("PlayerMain")) // Ensure only the main player bubble triggers the splitter
        {
            Blob playerBlob = collision.GetComponent<Blob>();
            if (playerBlob != null)
            {
                playerBlob.SplitBubbleUsingSplitter(transform.position);
            }
        }
    }
}