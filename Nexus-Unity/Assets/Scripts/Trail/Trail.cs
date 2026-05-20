using System.Collections.Generic;
using UnityEngine;
using static Trail;

[ExecuteInEditMode]
public class Trail : MonoBehaviour
{
    TileController tileController;

    [Header("Spawn Settings")]
    [SerializeField] int startX;
    [SerializeField] int startY;
    [SerializeField] float offset;

    [Header("Movement Settings")]
    [Tooltip("Speed in steps per second")]
    [Range(0f, 10f)]
    [SerializeField] public float speed = 1f;
    [Range(0, 1)]
    [SerializeField] public float speedRandomness = 0f;
    [Range(0f, 1f)]
    [SerializeField] float turnProbability = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] public float escapeProbability = 0;
    public enum EscapeDirection { Right, Left, Up, Down }
    public EscapeDirection[] escapeDirections;

    [SerializeField] public float maxLife = 1f;
    float timeAtStart = 0f;

    public float uniqueRandomness = 0f;

    [Header("Debug & Controls")]
    [SerializeField] bool reset;

    [Header("Rendering")]
    public float trailLength = 1f;
    public float trailWidth = 1f;

    // Current state for live path calculation
    private int currentX;
    private int currentY;
    private Vector2Int currentDir;
    private HashSet<Vector2Int> visitedTiles = new HashSet<Vector2Int>();
    private float stepTimer = 0f;
    private bool pathEnded = false;

    TrailRenderer trail;

    Vector3 prevPos;
    Vector3 targetPos;

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
        trail = GetComponent<TrailRenderer>();
        timeAtStart = Time.unscaledTime;

        if (tileController != null)
        {
            InitializePathState();
        }
    }

    void Update()
    {
        if (reset)
        {
            ResetTrail();
            reset = false;
        }

        // Safety check: Don't move if we have no tile controller*
        if (tileController == null) return;

        // Calculate age
        float age = Time.unscaledTime - timeAtStart;

        // Handle destruction based on life only
        if (age >= maxLife)
        {
            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);
            return;
        }

        // Update trail appearance based on life
        float lifeMap = Mathf.Clamp01(((age / maxLife) - .8f) / .2f);
        if (trail == null) trail = GetComponent<TrailRenderer>();
        trail.time = trailLength * (1 - lifeMap);
        trail.widthMultiplier = trailWidth;


        // Don't continue moving if path has ended
        if (pathEnded)
        {
            return;
        }

        // Step timer for live path calculation
        stepTimer += (speed - uniqueRandomness * speedRandomness) * Time.unscaledDeltaTime;

        // Take a step when timer reaches 1 (1 step per second at speed=1)
        if (stepTimer >= 1f)
        {
            stepTimer -= 1f;
            TakeStep();
        }

        Vector3 finalPos = Vector3.Lerp(prevPos, targetPos, stepTimer);
        transform.position = finalPos;
    }

    public void ResetTrail()
    {
        stepTimer = 0f;
        pathEnded = false;
        timeAtStart = Time.unscaledTime;
        InitializePathState();

        // Clear trail renderer
        if (trail == null) trail = GetComponent<TrailRenderer>();
        trail.Clear();
    }

    public void Init(TileController tileController, int startX, int startY, float offset)
    {
        this.tileController = tileController;
        this.startX = startX;
        this.startY = startY;
        this.offset = offset;
        uniqueRandomness = Random.value;
        timeAtStart = Time.unscaledTime;
        InitializePathState();
    }

    private void InitializePathState()
    {
        if (tileController == null)
        {
            Debug.LogError("TileController is not assigned!");
            return;
        }

        visitedTiles.Clear();
        currentX = startX;
        currentY = startY;
        prevPos = tileController.getPositionAt(currentX, currentY, true, offset);
        targetPos = prevPos;
        pathEnded = false;

        // Mark starting tile as visited
        visitedTiles.Add(new Vector2Int(currentX, currentY));

        // Set initial position
        transform.position = tileController.getPositionAt(currentX, currentY, true, offset);

        // Pick an initial random direction
        currentDir = directions[Random.Range(0, directions.Length)];
    }

    private void TakeStep()
    {
        // Get valid directions from current position
        List<Vector2Int> validDirections = GetValidDirections(currentX, currentY, visitedTiles);

        // If trapped, stop the path
        if (validDirections.Count == 0)
        {
            pathEnded = true;
            return;
        }

        // Decide whether to turn or keep going straight based on probability
        if (Random.value < turnProbability || !validDirections.Contains(currentDir))
        {
            bool usePreferredDir = Random.value < escapeProbability;

            if (usePreferredDir && escapeDirections != null && escapeDirections.Length > 0)
            {
                List<Vector2Int> preferred = new List<Vector2Int>();
                foreach (EscapeDirection escapeDir in escapeDirections)
                {
                    preferred.Add(directions[(int)escapeDir]);
                }
                List<Vector2Int> validPreferred = validDirections.FindAll(d => preferred.Contains(d));

                if (validPreferred.Count > 0)
                {
                    currentDir = validPreferred[Random.Range(0, validPreferred.Count)];
                }
                else
                {
                    pathEnded = true;
                    return;
                }
            }
            else
            {
                currentDir = validDirections[Random.Range(0, validDirections.Count)];
            }
        }

        // Move to the next position

        
        prevPos = tileController.getPositionAt(currentX, currentY, true, offset);
        currentX += currentDir.x;
        currentY += currentDir.y;

        // Mark new tile as visited

        visitedTiles.Add(new Vector2Int(currentX, currentY));

        // Update position
        targetPos = tileController.getPositionAt(currentX, currentY, true, offset);
        
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