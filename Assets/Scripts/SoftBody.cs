using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SoftBody : MonoBehaviour
{
    #region Constants
    private const float SplineOffset = 1f;
    #endregion

    #region Fields
    [SerializeField] private SpriteShapeController spriteShapeController;
    [SerializeField] private Transform[] controlPoints;
    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        UpdateVertices();
    }

    private void Update()
    {
        UpdateVertices();
    }
    #endregion

    #region Private Methods
    private void UpdateVertices()
    {
        for (int i = 0; i < controlPoints.Length - 1; i++)
        {
            // Get the position of the current control point
            Vector2 vertex = controlPoints[i].localPosition;

            // Calculate the direction from the center to the control point
            Vector2 directionToCenter = (Vector2.zero - vertex).normalized;

            // Get the radius of the collider attached to the control point
            float colliderRadius = controlPoints[i].GetComponent<CircleCollider2D>().radius;

            try
            {
                // Adjust the position of the spline point
                spriteShapeController.spline.SetPosition(i, vertex - directionToCenter * colliderRadius);
            }
            catch
            {
                // Handle cases where spline points are too close
                Debug.LogWarning("Spline points are too close. Adjusting...");
                spriteShapeController.spline.SetPosition(i, (vertex - directionToCenter * (colliderRadius + SplineOffset)));
            }
            // Get the current tangent magnitude from the spline
            float tangentMagnitude = spriteShapeController.spline.GetLeftTangent(i).magnitude;

// Calculate new tangents
            Vector2 newRightTangent = Vector2.Perpendicular(directionToCenter) * tangentMagnitude;
            Vector2 newLeftTangent = -newRightTangent;

// Assign tangents to the spline
            spriteShapeController.spline.SetRightTangent(i, newRightTangent);
            spriteShapeController.spline.SetLeftTangent(i, newLeftTangent);
        }
    }
    #endregion
}