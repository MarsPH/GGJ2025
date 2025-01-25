using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineCursorController : MonoBehaviour
{
    public string targetTag = "Bubble"; // Tag to identify the target objects
    public GameObject linePrefab;      // Prefab for the line
    private GameObject activeLine;     // The instance of the line
    public float lineLength = 2f;      // Length of the line
    public float rotationSpeed = 10f; // Speed for smooth rotation

    void Update()
    {
        // Check if the mouse button is held down
        if (Input.GetMouseButton(0))
        {
            // If the line doesn't exist, create it
            if (activeLine == null)
            {
                activeLine = Instantiate(linePrefab);
            }

            // Update the position of the line to follow the mouse
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            activeLine.transform.position = mousePosition;

            // Find the closest target with the specified tag
            GameObject targetObject = FindClosestTarget(mousePosition);

            if (targetObject != null)
            {
                Vector2 direction = (Vector2)targetObject.transform.position - mousePosition;

                // Swap the x and y in the direction to align horizontally
                float targetAngle = Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg;

                // Smoothly rotate towards the target angle
                float currentAngle = activeLine.transform.rotation.eulerAngles.z;
                float smoothAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
                activeLine.transform.rotation = Quaternion.Euler(0, 0, smoothAngle);
            }

            // Scale the line to the desired length
            activeLine.transform.localScale = new Vector3(lineLength, activeLine.transform.localScale.y, 1);
        }
        else
        {
            // Destroy the line when the mouse button is released
            if (activeLine != null)
            {
                Destroy(activeLine);
            }
        }
    }

    // Method to find the closest target based on the tag
    GameObject FindClosestTarget(Vector2 position)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject target in targets)
        {
            float distance = Vector2.Distance(position, target.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = target;
            }
        }

        return closest;
    }
}
