using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target; // Reference to the bubble or player
    public float borderThreshold = 0.1f; // Distance from the edge of the screen to trigger camera movement
    public float smoothSpeed = 0.1f; // How smooth the camera movement is

    private Vector3 velocity = Vector3.zero; // Used for smooth dampening

    void LateUpdate()
    {
        // Dynamically find the PlayerMain if the target is null or the target is no longer valid
        if (target == null || !target.CompareTag("PlayerMain"))
        {
            GameObject playerMain = GameObject.FindGameObjectWithTag("PlayerMain");
            if (playerMain != null)
            {
                target = playerMain.transform;
            }
            else
            {
                return; // No PlayerMain found, exit
            }
        }

        // Get the camera's boundaries in world space
        Camera camera = Camera.main;
        Vector3 viewportPosition = camera.WorldToViewportPoint(target.position);

        // Check if the bubble is near the edge of the screen
        Vector3 targetPosition = transform.position;

        if (viewportPosition.x < borderThreshold || viewportPosition.x > (1 - borderThreshold))
        {
            targetPosition.x = Mathf.Lerp(transform.position.x, target.position.x, smoothSpeed);
        }

        if (viewportPosition.y < borderThreshold || viewportPosition.y > (1 - borderThreshold))
        {
            targetPosition.y = Mathf.Lerp(transform.position.y, target.position.y, smoothSpeed);
        }

        // Smoothly move the camera to the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothSpeed);
    }

    // Method to manually update the camera target
    public void UpdateTarget(Transform newTarget)
    {
        target = newTarget;
    }
}