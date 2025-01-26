using UnityEngine;

public class ConstantForceZone : MonoBehaviour
{
    public Vector2 forceDirection = Vector2.left; // Direction of the force
    public float forceStrength = 5f; // Strength of the force
    public string targetTag = "Bubble"; // Tag of objects affected by the force

    private void OnTriggerStay2D(Collider2D other)
    {
        // Check if the object has the specified tag
        if (other.CompareTag(targetTag))
        {
            // Get the Rigidbody2D of the object
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Apply force in the specified direction
                rb.AddForce(forceDirection.normalized * forceStrength * rb.mass, ForceMode2D.Force);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize the force direction in the Scene view
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)forceDirection.normalized * 2);
    }
}