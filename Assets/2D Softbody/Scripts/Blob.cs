using System;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

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
            else if (collision.transform.CompareTag("Food"))
            {
                // Notify the Blob class to absorb food
                Blob parentBlob = transform.parent.GetComponent<Blob>();
                if (parentBlob != null)
                {
                    parentBlob.AbsorbFood(collision.transform.gameObject);
                }
            }
            
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                // Call DestroyBubble when colliding with a sharp object
                Blob parentBlob = transform.parent.GetComponent<Blob>();
                if (parentBlob != null)
                {
                    parentBlob.DestroyBubble();
                }
            }
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
    public PlayerRespawn playerRespawn;
    public bool isPlayer = false; // Set this to true for the player bubble

    public float maxDeformationThreshold = 0.5f; // Max allowable deformation
    public float minDeformationThreshold = 0.2f; // Min allowable deformation
    public string sharpObstacleTag = "SharpObstacle"; // Tag for sharp obstacles
    
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

    void Awake()
    {
        playerRespawn = GetComponent<PlayerRespawn>();
    }

    public void AbsorbBubble(Blob otherBlob, float absorberSize, float absorbedSize)
    {
        Debug.Log($"{name} is absorbing {otherBlob.name}");

        // Calculate the size of the new bubble
        float newSize = Mathf.Sqrt((absorberSize * absorberSize) + (absorbedSize * absorbedSize));

        // Store the velocity of the absorbing bubble
        Vector2 velocity = GetComponent<Rigidbody2D>().velocity;

        // Destroy the absorbed bubble
        Destroy(otherBlob.gameObject);

        // Instantiate a new larger bubble
        GameObject newBubble = SpawnNewBubble(transform.position, newSize);

        // Transfer the velocity to the new bubble
        Rigidbody2D newRb = newBubble.GetComponent<Rigidbody2D>();
        if (newRb != null)
        {
            newRb.velocity = velocity;
        }

        // Ensure the new bubble retains the "PlayerMain" tag
        newBubble.tag = "PlayerMain";
        SmoothCameraFollow cameraFollow = Camera.main.GetComponent<SmoothCameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.UpdateTarget(newBubble.transform);
        }

        // Destroy the current bubble
        Destroy(gameObject);
    }
    
    
    
    void AbsorbFood(GameObject foodObject)
    {
        Debug.Log($"{name} is eating food!");

        // Calculate the size increment based on the food object's scale
        float foodSize = foodObject.transform.localScale.x;
        float currentSize = transform.localScale.x;
        float newSize = Mathf.Sqrt((currentSize * currentSize) + (foodSize * foodSize));

        // Save the bubble's velocity
        Vector2 currentVelocity = GetComponent<Rigidbody2D>().velocity;

        // Destroy the current bubble and the food object
        Destroy(gameObject);
        Destroy(foodObject);

        // Spawn a new larger bubble
        GameObject newBubble = SpawnNewBubble(transform.position, newSize);

        // Transfer the velocity to the new bubble
        Rigidbody2D newRb = newBubble.GetComponent<Rigidbody2D>();
        if (newRb != null)
        {
            //newRb.velocity = currentVelocity;
        }
    }

    public GameObject SpawnNewBubble(Vector3 position, float size, bool isPlayer = false)
    {
        float safetyRadius = size * 0.6f;
        int maxAttempts = 50;
        int attempts = 0;

        while (Physics2D.OverlapCircle(position, safetyRadius) != null && attempts < maxAttempts)
        {
            position += new Vector3(Random.Range(-safetyRadius, safetyRadius), Random.Range(-safetyRadius, safetyRadius), 0);
            attempts++;
        }

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("Could not find a non-overlapping position, spawning at original position.");
        }

        GameObject bubblePrefab = Resources.Load<GameObject>("BubblePrefab");
        GameObject newBubble = Instantiate(bubblePrefab, position, Quaternion.identity);

        newBubble.transform.localScale = new Vector3(size, size, 1);
        Blob newBlob = newBubble.GetComponent<Blob>();
        newBubble.tag = isPlayer ? "PlayerMain" : "Bubble"; // Set the tag

        if (newBlob != null)
        {
            newBlob.referencePointDistance = this.referencePointDistance;
            newBlob.referencePointRadius = this.referencePointRadius;
            newBlob.springDampingRatio = this.springDampingRatio;
            newBlob.springFrequency = this.springFrequency;
            newBlob.mappingDetail = this.mappingDetail;
            newBlob.isPlayer = isPlayer; // Set the isPlayer flag
        }
        return newBubble;
    }

    
    void CheckForDeformation()
    {
        // Scale the max threshold proportionally to the bubble size
        float maxThreshold = maxDeformationThreshold * (transform.localScale.x); 

        // Scale the min threshold, but make smaller bubbles more forgiving
        float minThreshold = minDeformationThreshold * Mathf.Pow(transform.localScale.x / 8f, 0.0000000001f) ; 

        float maxDistance = 0f;
        float minDistance = float.MaxValue;

        foreach (GameObject referencePoint in referencePoints)
        {
            float distance = Vector2.Distance(referencePoint.transform.position, transform.position);
            maxDistance = Mathf.Max(maxDistance, distance);
            minDistance = Mathf.Min(minDistance, distance);
        }

        // Trigger death if deformation exceeds thresholds
        if (maxDistance > maxThreshold || minDistance < minThreshold)
        {
            DestroyBubble();
        }
    }

    public void DestroyBubble()
    {
        
        Debug.Log($"{gameObject.name} has popped!");
        // Optional: Add particle effects, sound, or other destruction feedback here
        playerRespawn.Respawn();

        Destroy(gameObject);

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
        if (isPlayer)
        {
            // Handle player-specific input
            if (Input.GetMouseButtonDown(1)) // Right-click to trigger the split
            {
                SplitBubble();
            }
        }
        if (transform.localScale != previousScale)
        {
            RecalculateOffsetsAndWeights();
            UpdateCollidersForScale();
            previousScale = transform.localScale;
        }
        UpdateVertexPositions();
        CheckForDeformation();
    }
    
    void SplitBubble()
    {
    if (transform.localScale.x <= 1.25f)
    {
        Debug.Log("Bubble is too small to split!");
        return;
    }

    float offsetMultiplier = 0.05f; // Adjusted to spawn bubbles closer

    // Calculate the size of the new bubbles
    float newSizePlayer = transform.localScale.x * 0.6f; // Bigger bubble for the player
    float newSizeNPC = transform.localScale.x * 0.4f; // Smaller bubble for the NPC

    // Define a random range for offsets
    float offsetRange = referencePointDistance * transform.localScale.x * offsetMultiplier;

    // Ensure offsetRange is large enough
    offsetRange = Mathf.Max(offsetRange, newSizePlayer * 0.75f);

    // Generate random offsets for the two new bubbles
    Vector3 offset1 = new Vector3(Random.Range(-offsetRange, offsetRange), Random.Range(-offsetRange, offsetRange), 0);
    Vector3 offset2 = new Vector3(Random.Range(-offsetRange, offsetRange), Random.Range(-offsetRange, offsetRange), 0);

    // Ensure offsets are not overlapping or too close
    int maxAttempts = 100; // Prevent infinite loops
    int attempts = 0;
    while (Vector3.Distance(offset1, offset2) < newSizePlayer && attempts < maxAttempts)
    {
        offset2 = new Vector3(Random.Range(-offsetRange, offsetRange), Random.Range(-offsetRange, offsetRange), 0);
        attempts++;
    }

    // If valid offsets couldn't be found, fallback positions
    if (attempts >= maxAttempts)
    {
        offset1 = new Vector3(-newSizePlayer * 1.5f, 0, 0);
        offset2 = new Vector3(newSizePlayer * 1.5f, 0, 0);
    }

    // Calculate spawn positions
    Vector3 spawnPosition1 = transform.position + offset1;
    Vector3 spawnPosition2 = transform.position + offset2;

    // Get the current velocity of the original bubble
    Vector2 originalVelocity = GetComponent<Rigidbody2D>().velocity;

    // Instantiate two new bubbles
    GameObject bubblePlayer = SpawnNewBubble(spawnPosition1, newSizePlayer, true); // Player bubble
    GameObject bubbleNPC = SpawnNewBubble(spawnPosition2, newSizeNPC, false); // NPC bubble

    // Apply velocity to the new bubbles
    Rigidbody2D rbPlayer = bubblePlayer.GetComponent<Rigidbody2D>();
    Rigidbody2D rbNPC = bubbleNPC.GetComponent<Rigidbody2D>();

    if (rbPlayer != null)
    {
        rbPlayer.velocity = originalVelocity; // Maintain original velocity
    }

    if (rbNPC != null)
    {
        rbNPC.velocity = originalVelocity * 0.8f; // Slightly slower for the NPC bubble
    }

    // Update camera to follow the new player bubble
    SmoothCameraFollow cameraFollow = Camera.main.GetComponent<SmoothCameraFollow>();
    if (cameraFollow != null)
    {
        cameraFollow.UpdateTarget(bubblePlayer.transform);
    }

    // Destroy the current bubble
    Destroy(gameObject);
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

