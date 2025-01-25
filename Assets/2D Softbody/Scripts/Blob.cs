﻿using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Blob : MonoBehaviour
{
    private class PropagateCollisions : MonoBehaviour
    {
        private bool hasAbsorbed = false;
        GameObject zPlayer;
        private void Start()
        {
            zPlayer = GameObject.FindGameObjectWithTag("PlayerMain");
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.transform.CompareTag("Bubble"))
            {
                // Get the main Blob component from both objects
                Blob thisBlob = transform.parent.GetComponent<Blob>();
                Blob otherBlob = collision.transform.parent.GetComponent<Blob>();

                if (thisBlob != null && otherBlob != null)
                {
                    // Calculate sizes of both bubbles
                    float thisSize = thisBlob.GetBubbleSize();
                    float otherSize = otherBlob.GetBubbleSize();

                    // Ensure only one bubble handles the absorption
                    if (thisSize > otherSize && !thisBlob.hasAbsorbed)
                    {
                        thisBlob.hasAbsorbed = true;
                        thisBlob.AbsorbBubble(otherBlob, thisSize, otherSize);
                    }
                    else if (thisSize < otherSize && !otherBlob.hasAbsorbed)
                    {
                        otherBlob.hasAbsorbed = true;
                        otherBlob.AbsorbBubble(thisBlob, otherSize, thisSize);
                    }
                }
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.transform.tag == "DetachOne")
            {
                gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;

            }
            if (collision.transform.tag == "DetachAll")
            {
                zPlayer.GetComponent<Blob>().TrigThis();

            }
        }
    }
    private Vector3 previousScale;
    public float referencePointDistance = 0.5f;
    public bool hasAbsorbed = false;
    public int width = 5;
    public int height = 5;
    public int referencePointsCount = 12;
    public float referencePointRadius = 0.25f;
    public float mappingDetail = 10;
    public float springDampingRatio = 0;
    public float springFrequency = 2;
    public PhysicsMaterial2D surfaceMaterial;
    public Rigidbody2D[] allReferencePoints;
    GameObject[] referencePoints;
    int vertexCount;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uv;
    Vector3[,] offsets;
    float[,] weights;

    void Start()
    {
        previousScale = transform.localScale;

        CreateReferencePoints();
        CreateMesh();
        MapVerticesToReferencePoints();

    }
    
    void RecalculateSpringJoints()
    {
        // Iterate over all reference points
        for (int i = 0; i < referencePointsCount; i++)
        {
            SpringJoint2D springJoint = referencePoints[i].GetComponent<SpringJoint2D>();

            // Connect each point to the main bubble
            springJoint.connectedBody = GetComponent<Rigidbody2D>();
            springJoint.connectedAnchor = Vector3.zero;
            springJoint.distance = 0;
            springJoint.dampingRatio = springDampingRatio;
            springJoint.frequency = springFrequency;

            // Connect adjacent reference points to form a circular structure
            if (i > 0)
            {
                SpringJoint2D adjacentJoint = referencePoints[i].AddComponent<SpringJoint2D>();
                adjacentJoint.connectedBody = referencePoints[i - 1].GetComponent<Rigidbody2D>();
                adjacentJoint.distance = 0;
                adjacentJoint.dampingRatio = springDampingRatio;
                adjacentJoint.frequency = springFrequency;
            }
        }

        // Close the circular structure by connecting the last point to the first
        SpringJoint2D finalJoint = referencePoints[0].AddComponent<SpringJoint2D>();
        finalJoint.connectedBody = referencePoints[referencePointsCount - 1].GetComponent<Rigidbody2D>();
        finalJoint.distance = 0;
        finalJoint.dampingRatio = springDampingRatio;
        finalJoint.frequency = springFrequency;
    }


    void UpdateReferencePointsAfterAbsorption(float newBubbleSize)
    {
        float angleStep = 360f / referencePointsCount; // Divide full circle
        float radius = referencePointDistance * 0.8f * (newBubbleSize / transform.localScale.x); // Adjust radius closer to center

        for (int i = 0; i < referencePointsCount; i++)
        {
            // Calculate new position in local space
            float angle = i * angleStep * Mathf.Deg2Rad; // Convert to radians
            Vector3 newPosition = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0
            );

            // Update reference point's position relative to the blob
            referencePoints[i].transform.localPosition = newPosition;

            // Reset local rotation to maintain proper alignment
            referencePoints[i].transform.localRotation = Quaternion.identity; // Ensures consistent Z rotation

            // Adjust collider radius dynamically
            CircleCollider2D collider = referencePoints[i].GetComponent<CircleCollider2D>();
            if (collider != null)
            {
                collider.radius = referencePointRadius * 0.8f;
            }
        }

        // Recalculate offsets and update the mesh
        RecalculateOffsetsAndWeights();
        UpdateVertexPositions();
    }


    
    
    public void AbsorbBubble(Blob otherBlob, float absorberSize, float absorbedSize)
    {
        // Debug message for absorption
        Debug.Log($"{name} is absorbing {otherBlob.name}");

        // Calculate the size of the new bubble
        float newSize = Mathf.Sqrt((absorberSize * absorberSize) + (absorbedSize * absorbedSize));

        // Calculate the position for the new bubble (centered between the two bubbles)
        Vector3 newPosition = (transform.position + otherBlob.transform.position) / 2;

        // Destroy the current bubbles
        Destroy(gameObject);
        Destroy(otherBlob.gameObject);

        // Instantiate a new larger bubble
        SpawnNewBubble(newPosition, newSize);
    }
    private void SpawnNewBubble(Vector3 position, float size)
    {
        // Reference to your prefab
        GameObject bubblePrefab = Resources.Load<GameObject>("BubblePrefab");

        // Spawn a new bubble
        GameObject newBubble = Instantiate(bubblePrefab, position, Quaternion.identity);

        // Scale the new bubble appropriately
        newBubble.transform.localScale = new Vector3(size, size, 1);

        // Optionally, set other properties on the new bubble
        Blob newBlob = newBubble.GetComponent<Blob>();
        newBubble.tag = "Bubble";
        if (newBlob != null)
        {
            newBlob.referencePointDistance = this.referencePointDistance;
            newBlob.referencePointRadius = this.referencePointRadius;
            newBlob.springDampingRatio = this.springDampingRatio;
            newBlob.springFrequency = this.springFrequency;
            newBlob.mappingDetail = this.mappingDetail;
        }
    }
    
    
    void OnDrawGizmos()
    {
        // Ensure reference points are initialized
        if (referencePoints != null)
        {
            Gizmos.color = Color.cyan; // Set the color for the reference points

            foreach (GameObject referencePoint in referencePoints)
            {
                if (referencePoint != null)
                {
                    // Draw a sphere at each reference point's position
                    Gizmos.DrawSphere(referencePoint.transform.position, 1f); // Adjust size as needed
                }
            }
        }
    }


    public float GetBubbleSize()
    {
        // Use the overall scale as a simple proxy for size
        return transform.localScale.x;
    }
    void CreateReferencePoints()
    {
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        referencePoints = new GameObject[referencePointsCount];
        Vector3 offsetFromCenter = ((referencePointDistance - referencePointRadius) * Vector3.up);
        float angle = 360.0f / referencePointsCount;

        for (int i = 0; i < referencePointsCount; i++)
        {
            referencePoints[i] = new GameObject();
            referencePoints[i].tag = gameObject.tag;
            referencePoints[i].AddComponent<PropagateCollisions>();
            referencePoints[i].transform.parent = transform;

            Quaternion rotation = Quaternion.AngleAxis(angle * (i - 1), Vector3.back);
            referencePoints[i].transform.localPosition = rotation * offsetFromCenter;

            Rigidbody2D body = referencePoints[i].AddComponent<Rigidbody2D>();
            body.constraints = RigidbodyConstraints2D.None;
            //body.constraints = RigidbodyConstraints2D.FreezeRotation; // Lock Z-axis rotation
            body.mass = 0.5f;
            body.interpolation = rigidbody.interpolation;
            body.collisionDetectionMode = rigidbody.collisionDetectionMode;
            allReferencePoints[i] = body;

            CircleCollider2D collider = referencePoints[i].AddComponent<CircleCollider2D>();
            collider.radius = referencePointRadius * (transform.localScale.x / 2);
            if (surfaceMaterial != null)
            {
                collider.sharedMaterial = surfaceMaterial;
            }

            AttachWithSpringJoint(referencePoints[i], gameObject);
            if (i > 0)
            {
                AttachWithSpringJoint(referencePoints[i], referencePoints[i - 1]);
            }
        }
        AttachWithSpringJoint(referencePoints[0], referencePoints[referencePointsCount - 1]);

        IgnoreCollisionsBetweenReferencePoints();
    }

    void AttachWithSpringJoint(GameObject referencePoint,
            GameObject connected)
    {
        SpringJoint2D springJoint =
            referencePoint.AddComponent<SpringJoint2D>();
        springJoint.connectedBody = connected.GetComponent<Rigidbody2D>();
        springJoint.connectedAnchor = LocalPosition(referencePoint) -
            LocalPosition(connected);
        springJoint.distance = 0;
        springJoint.dampingRatio = springDampingRatio;
        springJoint.frequency = springFrequency;
    }

    void IgnoreCollisionsBetweenReferencePoints()
    {
        int i;
        int j;
        CircleCollider2D a;
        CircleCollider2D b;

        for (i = 0; i < referencePointsCount; i++)
        {
            for (j = i; j < referencePointsCount; j++)
            {
                a = referencePoints[i].GetComponent<CircleCollider2D>();
                b = referencePoints[j].GetComponent<CircleCollider2D>();
                Physics2D.IgnoreCollision(a, b, true);
            }
        }
    }

    void CreateMesh()
    {
        vertexCount = (width + 1) * (height + 1);

        int trianglesCount = width * height * 6;
        vertices = new Vector3[vertexCount];
        triangles = new int[trianglesCount];
        uv = new Vector2[vertexCount];
        int t;

        for (int y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++)
            {
                int v = (width + 1) * y + x;
                vertices[v] = new Vector3(x / (float)width - 0.5f,
                        y / (float)height - 0.5f, 0);
                uv[v] = new Vector2(x / (float)width, y / (float)height);

                if (x < width && y < height)
                {
                    t = 3 * (2 * width * y + 2 * x);

                    triangles[t] = v;
                    triangles[++t] = v + width + 1;
                    triangles[++t] = v + width + 2;
                    triangles[++t] = v;
                    triangles[++t] = v + width + 2;
                    triangles[++t] = v + 1;
                }
            }
        }

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    void MapVerticesToReferencePoints()
    {
        offsets = new Vector3[vertexCount, referencePointsCount];
        weights = new float[vertexCount, referencePointsCount];

        for (int i = 0; i < vertexCount; i++)
        {
            float totalWeight = 0;

            for (int j = 0; j < referencePointsCount; j++)
            {
                offsets[i, j] = vertices[i] - LocalPosition(referencePoints[j]);
                weights[i, j] =
                    1 / Mathf.Pow(offsets[i, j].magnitude, mappingDetail);
                totalWeight += weights[i, j];
            }

            for (int j = 0; j < referencePointsCount; j++)
            {
                weights[i, j] /= totalWeight;
            }
        }
    }

    void Update()
    {
        if (transform.localScale != previousScale)
        {
            RecalculateOffsetsAndWeights();
            UpdateCollidersForScale();
            previousScale = transform.localScale;
        }
        UpdateVertexPositions();
        if (Input.GetMouseButtonDown(0))
        {

                // Add force to move the blob to the right
                /*
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                rb.AddForce(Vector2.right * 10f, ForceMode2D.Impulse); // Adjust the force multiplier as needed
                */
   
        }
    }

    void UpdateVertexPositions()
    {
        Vector3[] updatedVertices = new Vector3[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            updatedVertices[i] = Vector3.zero;

            for (int j = 0; j < referencePointsCount; j++)
            {
                // Use local positions and offsets to calculate vertex positions
                Vector3 targetPosition = referencePoints[j].transform.localPosition + offsets[i, j];
                updatedVertices[i] += weights[i, j] * targetPosition;
            }
        }

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.vertices = updatedVertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    Vector3 LocalPosition(GameObject obj)
    {
        return transform.InverseTransformPoint(obj.transform.position);
    }



    public void TrigThis()
    {
        int z = 0;
        foreach (Rigidbody2D child in allReferencePoints)
        {
            allReferencePoints[z].constraints = RigidbodyConstraints2D.None;
            z++;
        }
    }
    
    void RecalculateOffsetsAndWeights()
    {
        for (int i = 0; i < vertexCount; i++)
        {
            float totalWeight = 0;

            for (int j = 0; j < referencePointsCount; j++)
            {
                // Use local positions for consistent offset calculation
                offsets[i, j] = vertices[i] - referencePoints[j].transform.localPosition;
                float distance = offsets[i, j].magnitude + 0.0001f; // Prevent division by zero
                weights[i, j] = 1 / Mathf.Pow(distance, mappingDetail);
                totalWeight += weights[i, j];
            }

            // Normalize weights to maintain balance
            for (int j = 0; j < referencePointsCount; j++)
            {
                weights[i, j] /= totalWeight;
            }
        }
    }

    void UpdateCollidersForScale()
    {
        foreach (GameObject referencePoint in referencePoints)
        {
            CircleCollider2D collider = referencePoint.GetComponent<CircleCollider2D>();
            collider.radius = referencePointRadius * (transform.localScale.x / 2);
        }
    }

}

