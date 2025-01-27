using System;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
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
                    if (thisSize > otherSize && !thisBlob.hasAbsorbed )
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
            if (collision.CompareTag("Splitter"))
            {
                Debug.Log("Entered Splitter Trigger!");
                // Access the parent Blob script and call SplitBubbleUsingSplitter
                Blob parentBlob = transform.parent.GetComponent<Blob>();
                if (parentBlob != null)
                {
                    parentBlob.SplitBubbleUsingSplitter(collision.transform.position);
                }
            }
            if (collision.CompareTag("Enemy"))
            {
                // Call DestroyBubble when colliding with a sharp object
                Blob parentBlob = transform.parent.GetComponent<Blob>();
                if (parentBlob != null)
                {
                    parentBlob.TakeDamage();
                }
            }
            else if (collision.CompareTag("Collectible"))
            {
                Blob parentBlob = transform.parent.GetComponent<Blob>();
                if (parentBlob != null)
                {
                    parentBlob.CollectItem(collision.gameObject);
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
    public void UpdateHealthUI()
    {
        //healthSlider.value = (float)GameManager.Instance.CurrentHearts / GameManager.Instance.MaxHearts;
    }
    
    public void LoseGame()
    {
        Debug.Log("Game Over!");
        // Add code to transition to a game over scene or display lose UI
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
    }
    public Slider healthSlider; // Assign via the Inspector
    public Image splitIndicator; // Reference to the SplitIndicator UI image
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
    
    public int totalCollectibles = 5; // Total collectibles to win
    private int collectedCount = 0;

    public void CollectItem(GameObject collectible)
    {
        // Ensure the collectible isn't collected more than once
    Collider2D collectibleCollider = collectible.GetComponent<Collider2D>();

    if (collectibleCollider == null || !collectibleCollider.enabled)
    {
        return; // Skip if already collected
    }

    // Disable the collider to prevent further triggers
    collectibleCollider.enabled = false;

    // Notify the GameManager about the collected item
    if (GameManager.Instance != null)
    {
        GameManager.Instance.CollectItem();
    }
    else
    {
        Debug.LogWarning("GameManager instance is null!");
    }

    // Destroy the collectible
    Destroy(collectible);
    }
    
    public int maxHearts = 3; // Maximum number of hearts
    private int currentHearts;
    public void WinGame()
    {
        Debug.Log("You Win!");
        // Add code to transition to a win scene or display win UI
        UnityEngine.SceneManagement.SceneManager.LoadScene("WinScene");
    }
    void Start()
    {
        currentHearts = maxHearts; // Initialize hearts
        if (splitIndicator == null)
        {
            //Debug.LogError("SplitIndicator UI is not assigned in the Blob script.");
        }
        previousScale = transform.localScale;

        CreateReferencePoints();
        CreateMesh();
        MapVerticesToReferencePoints();

    }
    private float damageCooldown = 1f; // 1 second cooldown
    private float lastDamageTime = -1f; // Tracks the last time damage was taken

    public void TakeDamage()
    {
        // Check if cooldown has passed
        if (Time.time - lastDamageTime < damageCooldown)
        {
            return; // Exit if still in cooldown
        }

        lastDamageTime = Time.time; // Update last damage time

        Debug.Log($"Bubble took damage! Hearts left: {GameManager.Instance.CurrentHearts}");
        DestroyBubble();
    }

    void Awake()
    {
        playerRespawn = GetComponent<PlayerRespawn>();
    }

    public void AbsorbBubble(Blob otherBlob, float absorberSize, float absorbedSize)
    {
        if (!isPlayer)
        {
            return;
        }
        Debug.Log($"{name} is absorbing {otherBlob.name}");

        // Calculate the size of the new bubble
        float newSize = Mathf.Sqrt((absorberSize * absorberSize) + (absorbedSize * absorbedSize));

        // Store the velocity of the absorbing bubble
        Vector2 originalVelocity = GetComponent<Rigidbody2D>().velocity;

        // Destroy the absorbed bubble
        Destroy(otherBlob.gameObject);

        // Find a safe position for the new bubble
        Vector3 spawnPosition = FindSafePosition(transform.position, newSize * 0.6f);

        // Instantiate a new larger bubble
        GameObject newBubble = SpawnNewBubble(spawnPosition, newSize, true);

        // Transfer the velocity to the new bubble
        Rigidbody2D newRb = newBubble.GetComponent<Rigidbody2D>();
        if (newRb != null)
        {
            newRb.velocity = originalVelocity;
        }

        // Update the camera to follow the new bubble if it is the player
        if (isPlayer)
        {
            SmoothCameraFollow cameraFollow = Camera.main.GetComponent<SmoothCameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.UpdateTarget(newBubble.transform);
            }
        }

        // Destroy the current bubble
        Destroy(gameObject);
    }
    
   
    
    private Vector3 FindSafePosition(Vector3 originalPosition, float safetyRadius)
    {
        int maxAttempts = 50; // Limit attempts to prevent infinite loops
        for (int attempts = 0; attempts < maxAttempts; attempts++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-safetyRadius, safetyRadius),
                Random.Range(-safetyRadius, safetyRadius),
                0
            );

            Vector3 testPosition = originalPosition + offset;

            // Check for overlapping colliders, including the splitter
            Collider2D[] colliders = Physics2D.OverlapCircleAll(testPosition, safetyRadius);
            bool hasCollision = false;

            foreach (Collider2D collider in colliders)
            {
                if (collider.gameObject.CompareTag("Splitter") || collider.gameObject != this.gameObject)
                {
                    hasCollision = true;
                    break;
                }
            }

            if (!hasCollision)
            {
                return testPosition; // Valid position found
            }
        }

        //Debug.LogWarning("Could not find a safe position for bubble splitting.");
        return originalPosition; // Fallback
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
        GameObject newBubble = SpawnNewBubble(transform.position, newSize, true);

        // Transfer the velocity to the new bubble
        Rigidbody2D newRb = newBubble.GetComponent<Rigidbody2D>();
        if (newRb != null)
        {
            //newRb.velocity = currentVelocity;
        }
    }
    
    public void SplitBubbleUsingSplitter(Vector3 splitterPosition)
    {
        if (transform.localScale.x <= 1.25f)
        {
            Debug.Log("Bubble is too small to split!");
            return;
        }

        // Fetch spawn points from the splitter
        Collider2D splitterCollider = Physics2D.OverlapPoint(splitterPosition);
        if (splitterCollider == null || !splitterCollider.CompareTag("Splitter"))
        {
            Debug.LogWarning("Splitter not found or invalid.");
            return;
        }

        Transform splitterTransform = splitterCollider.transform;
        Transform spawnPoint1 = splitterTransform.Find("SpawnPoint1");
        Transform spawnPoint2 = splitterTransform.Find("SpawnPoint2");

        if (spawnPoint1 == null || spawnPoint2 == null)
        {
            Debug.LogError("Splitter does not have assigned spawn points.");
            return;
        }

        float newSizePlayer = transform.localScale.x * 0.6f; // Player bubble size
        float newSizeNPC = transform.localScale.x * 0.4f;   // NPC bubble size

        Vector2 originalVelocity = GetComponent<Rigidbody2D>().velocity;

        // Use predefined spawn points
        GameObject bubblePlayer = SpawnNewBubble(spawnPoint1.position, newSizePlayer, true);
        GameObject bubbleNPC = SpawnNewBubble(spawnPoint2.position, newSizeNPC, false);

        // Apply velocities to new bubbles
        Rigidbody2D rbPlayer = bubblePlayer.GetComponent<Rigidbody2D>();
        Rigidbody2D rbNPC = bubbleNPC.GetComponent<Rigidbody2D>();

        if (rbPlayer != null)
        {
            rbPlayer.velocity = originalVelocity;
        }

        if (rbNPC != null)
        {
            rbNPC.velocity = originalVelocity * 0.8f; // Slightly slower NPC
        }

        // Update camera to follow the new player
        SmoothCameraFollow cameraFollow = Camera.main.GetComponent<SmoothCameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.UpdateTarget(bubblePlayer.transform);
        }

        // Destroy the splitter after splitting
        Destroy(splitterTransform.gameObject);

        // Destroy the current bubble
        Destroy(gameObject);
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
    
    private float stressLevel = 0f; // Current stress level (0 to 1)
    private float stressDecayRate = 0.2f; // How quickly stress decreases over time
    private float stressIncreaseRate = 0.1f; // How much stress increases for deformation
    private float stressThreshold = 0.3f; // Stress level at which the bubble pops

    
    void CheckForDeformation()
    {
        // Calculate the dynamic thresholds based on the bubble's size
        float maxThreshold = maxDeformationThreshold * transform.localScale.x * 1.5f; // Increase max threshold
        float minThreshold = minDeformationThreshold * Mathf.Pow(transform.localScale.x / 8f, 0.25f); // Smaller bubbles are more forgiving

        float maxDistance = 0f;
        float minDistance = float.MaxValue;

        // Iterate through reference points to calculate distances
        foreach (GameObject referencePoint in referencePoints)
        {
            float distance = Vector2.Distance(referencePoint.transform.position, transform.position);
            maxDistance = Mathf.Max(maxDistance, distance);
            minDistance = Mathf.Min(minDistance, distance);

            // Check if the distance exceeds the thresholds
            if (distance > maxThreshold || distance < minThreshold)
            {
                // Accumulate stress based on how far out of bounds the distance is
                float deformation = Mathf.Max(Mathf.Abs(distance - maxThreshold), Mathf.Abs(minThreshold - distance));
                stressLevel += deformation * stressIncreaseRate * Time.deltaTime;
            }
        }

        // Decay stress over time (makes the mechanic forgiving)
        stressLevel = Mathf.Max(0f, stressLevel - stressDecayRate * Time.deltaTime);

        // Visual or audio warning when stress level is high
        if (stressLevel > 0.7f * stressThreshold)
        {
            Debug.Log("Bubble is under stress! Be careful.");
        }

        // Pop the bubble if the stress threshold is exceeded
        if (stressLevel >= stressThreshold)
        {
            DestroyBubble();
        }
    }

    public void DestroyBubble()
    {
        Debug.Log($"{gameObject.name} has popped!");

        // Use HealthManager or GameManager to handle health reduction and UI updates
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DecreaseHeart(); // Handles hearts and UI updates
        }
        else
        {
            Debug.LogWarning("GameManager instance is null!");
        }

        // Check for Game Over
        if (GameManager.Instance.CurrentHearts <= 0)
        {
            Debug.Log("No hearts left. Game Over!");
            GameManager.Instance.GameOver();
            Destroy(gameObject);
            return;
        }

        // Get the checkpoint position
        Vector3 respawnPosition = CheckpointManager.Instance.GetCheckpointPosition();
        Debug.Log($"Respawning at checkpoint: {respawnPosition}");

        if (respawnPosition != Vector3.zero)
        {
            // Respawn the player at the checkpoint
            GameObject newBubble = SpawnNewBubble(respawnPosition, transform.localScale.x, true);

            // Update the camera to follow the new bubble
            SmoothCameraFollow cameraFollow = Camera.main.GetComponent<SmoothCameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.UpdateTarget(newBubble.transform);
            }

            // Destroy the current bubble
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("Checkpoint position not found!");
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
        //UpdateSplitIndicator();
        if (isPlayer)
        {
            // Update split indicator status
            UpdateSplitIndicator();

            // Handle splitting if possible
            if (Input.GetMouseButtonDown(1)) // Right-click to split
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
    
    void UpdateSplitIndicator()
    {
        if (splitIndicator == null) return;

        // Green if split is possible; red otherwise
        splitIndicator.color = CanSplitBubble() ? Color.green : Color.red;
    }
    
    bool CanSplitBubble()
    {
        if (transform.localScale.x <= 1.25f)
        {
            Debug.Log("Bubble too small to split.");
            return false;
        }

        // Test for valid positions
        float newSizePlayer = transform.localScale.x * 0.6f;
        float newSizeNPC = transform.localScale.x * 0.4f;
        Vector3 spawnPosition1 = FindSafePosition(transform.position, newSizePlayer * 0.6f);
        Vector3 spawnPosition2 = FindSafePosition(transform.position, newSizeNPC * 0.6f);

        // If both positions are valid
        return spawnPosition1 != transform.position && spawnPosition2 != transform.position;
    }
    
    void SplitBubble()
{
    if (transform.localScale.x <= 1.25f)
    {
        Debug.Log("Bubble is too small to split!");
        return;
    }

    float newSizePlayer = transform.localScale.x * 0.6f; // Player bubble size
    float newSizeNPC = transform.localScale.x * 0.4f;   // NPC bubble size

    // Attempt to find valid positions for the bubbles
    Vector3 spawnPosition1 = FindSafePosition(transform.position, newSizePlayer * 0.6f);
    Vector3 spawnPosition2 = FindSafePosition(transform.position, newSizeNPC * 0.6f);

    if (spawnPosition1 == transform.position || spawnPosition2 == transform.position)
    {
        Debug.LogWarning("Failed to find valid split positions.");
        return;
    }

    Vector2 originalVelocity = GetComponent<Rigidbody2D>().velocity;

    // Spawn the player and NPC bubbles
    GameObject bubblePlayer = SpawnNewBubble(spawnPosition1, newSizePlayer, true);
    GameObject bubbleNPC = SpawnNewBubble(spawnPosition2, newSizeNPC, false);

    // Apply velocities to new bubbles
    Rigidbody2D rbPlayer = bubblePlayer.GetComponent<Rigidbody2D>();
    Rigidbody2D rbNPC = bubbleNPC.GetComponent<Rigidbody2D>();

    if (rbPlayer != null)
    {
        rbPlayer.velocity = originalVelocity;
    }

    if (rbNPC != null)
    {
        rbNPC.velocity = originalVelocity * 0.8f; // Slightly slower NPC
    }

    // Update camera to follow the new player
    SmoothCameraFollow cameraFollow = Camera.main.GetComponent<SmoothCameraFollow>();
    if (cameraFollow != null)
    {
        cameraFollow.UpdateTarget(bubblePlayer.transform);
    }

    // Destroy the current bubble
    Destroy(gameObject);
}
    // This triggers the "too small to split" visual effect
    void TriggerSplitNotAllowedVisual()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", Color.red); // Adjust to your shader's color property
            renderer.SetPropertyBlock(propertyBlock);

            Invoke(nameof(ResetSplitVisual), 0.5f); // Reset after 0.5 seconds
        }
    }

    void ResetSplitVisual()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", Color.white); // Reset to original color
            renderer.SetPropertyBlock(propertyBlock);
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

