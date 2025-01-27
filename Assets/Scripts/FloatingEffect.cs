using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    public float floatAmplitude = 0.5f; // How far up and down the object moves
    public float floatFrequency = 1f;  // How fast the object moves

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position; // Save the initial position
    }

    void Update()
    {
        // Calculate the new position
        Vector3 newPosition = startPosition;
        newPosition.y += Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        // Apply the position
        transform.position = newPosition;
    }
}
