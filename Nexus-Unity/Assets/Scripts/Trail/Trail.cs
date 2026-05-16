using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Trail : MonoBehaviour
{
    TileController tileController;

    [Header("Spawn Settings")]
    [SerializeField] int startX;
    [SerializeField] int startY;
    [SerializeField] float offset;

    [Header("Movement Settings")]
    [Tooltip("Time in seconds to complete the entire path")]
    [SerializeField] float runTime = 5f;
    [SerializeField] float runTimeRandomness = 0f;
    float randomizedRunTime;
    [Range(0f, 1f)]
    [SerializeField] float turnProbability = 0.5f;
    [SerializeField] int maxSteps = 20;


    [Header("Debug & Controls")]
    [SerializeField] bool reset;
    [Range(0f, 1f)]
    [SerializeField] float progression = 0f;

    [Header("Rendering")]
    public float trailLength = 1f;
    float timeAtFinish = 0f;

    // Internal list to hold the calculated world positions of the path
    List<Vector3> positions = new List<Vector3>();

    // 4 possible grid directions: Right, Left, Up, Down
    private readonly Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    void Start()
    {
        if (tileController != null)
        {
            GeneratePath();
        }
    }

    void Update()
    {
        if (reset)
        {
            ResetTrail();
            reset = false;
        }

        // Safety check: Don't move if we have no path generated
        if (positions == null || positions.Count < 2) return;

        // Progress the movement over time
        progression = Mathf.Clamp01(progression + Time.deltaTime / randomizedRunTime);

        // Calculate current index and the next index in the positions list
        float virtualIndex = progression * (positions.Count - 1);
        int progIndex = Mathf.FloorToInt(virtualIndex);

        Vector3 pos1 = positions[progIndex];
        Vector3 pos2 = positions[Mathf.Min(progIndex + 1, positions.Count - 1)];

        // Linearly interpolate between the two points
        float segmentProgression = virtualIndex % 1f;
        Vector3 targetPosition = Vector3.Lerp(pos1, pos2, segmentProgression);

        // Update the actual GameObject position to move it
        transform.position = targetPosition;

        // Handle trail destruction after completing the path
        if (progression >= 1f)
        {
            if (timeAtFinish == 0f)
            {
                timeAtFinish = Time.time;
            }
            else if (Time.time - timeAtFinish > trailLength)
            {
                if (Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
            }
            // REMOVED: The 'else { timeAtFinish = 0f; }' block was resetting the timer 
            // every frame after the first, preventing destruction.
        }
    }

    public void ResetTrail()
    {
        progression = 0f;
        timeAtFinish = 0f; // Reset the destruction timer
        GeneratePath();
    }

    public void Init(TileController tileController, int startX, int startY, float offset)
    {
        this.tileController = tileController;
        this.startX = startX;
        this.startY = startY;
        this.offset = offset;
        randomizedRunTime = runTime + Random.Range(-runTimeRandomness, runTimeRandomness);
        GeneratePath();
    }

    public void GeneratePath()
    {
        if (tileController == null)
        {
            Debug.LogError("TileController is not assigned!");
            return;
        }

        positions.Clear();

        // Track the grid positions we have already visited during this generation
        HashSet<Vector2Int> visitedTiles = new HashSet<Vector2Int>();

        int currentX = startX;
        int currentY = startY;

        // Mark the starting tile as visited and add its world position
        visitedTiles.Add(new Vector2Int(currentX, currentY));
        positions.Add(tileController.getPositionAt(currentX, currentY, true, offset));

        // Pick an initial random direction to start moving
        Vector2Int currentDir = directions[Random.Range(0, directions.Length)];

        bool pathFinding = true;
        int stepsTaken = 0;
        while (pathFinding)
        {
            // Pass the visited list to filter out backtracking
            List<Vector2Int> validDirections = GetValidDirections(currentX, currentY, visitedTiles);

            // If trapped (walls, boundaries, or self-intersections), stop.
            if (validDirections.Count == 0)
            {
                pathFinding = false;
                break;
            }

            // Decide whether to turn or keep going straight based on probability
            if (Random.value < turnProbability || !validDirections.Contains(currentDir))
            {
                currentDir = validDirections[Random.Range(0, validDirections.Count)];
            }

            // Move coordinates forward
            currentX += currentDir.x;
            currentY += currentDir.y;

            // Remember this tile so we don't return here
            visitedTiles.Add(new Vector2Int(currentX, currentY));
            positions.Add(tileController.getPositionAt(currentX, currentY,true, offset));


            if (stepsTaken >= maxSteps)
            {
                break;
            }
            stepsTaken++;
        }
    }

    /// <summary>
    /// Checks all 4 adjacent tiles to see which ones are valid moves and haven't been visited.
    /// </summary>
    private List<Vector2Int> GetValidDirections(int x, int y, HashSet<Vector2Int> visitedTiles)
    {
        List<Vector2Int> validDirs = new List<Vector2Int>();

        foreach (Vector2Int dir in directions)
        {
            int targetX = x + dir.x;
            int targetY = y + dir.y;
            Vector2Int targetPos = new Vector2Int(targetX, targetY);

            // Check if the tile is within map rules AND hasn't been stepped on yet
            if (tileController.IsValidTile(targetX, targetY) && !visitedTiles.Contains(targetPos))
            {
                validDirs.Add(dir);
            }
        }

        return validDirs;
    }
}