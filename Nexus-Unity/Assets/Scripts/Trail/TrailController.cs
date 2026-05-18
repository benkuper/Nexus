using System.Collections.Generic;
using UnityEngine;

[OSCQuery.DoNotExposeChildren]
[ExecuteInEditMode]
public class TrailController : MonoBehaviour
{
    public GameObject trailPrefab;
    public TileController tileController;

    public float spawnRate;
    float timeAtLastSpawn = 0f;

    public bool continuousSpawn;
    public int trailsPerSpawn = 1;
    public int continuousSpawnIndex = -1;

    public float offset = 0f;

    [Header("Trail Rendering")]
    public float trailWidth = 1f;
    public float trailLength = 1f;
    public float trailLife = 1f;
    [Range(0, 1)]
    public float trailLifeRandomness = .2f;
    [Range(0, 20)]
    public float trailSpeed = .1f;
    [Range(0f, 1f)]
    public float trailSpeedRandomness = 0f;
    [Range(0f, 1f)]
    public float trailEscapeProbability = 0f;
    public List<Trail.EscapeDirection> trailEscapeDirections;

    public bool clearTrails;

    [System.Serializable]
    public struct TilePosition
    {
        public int x, y;
    }

    [SerializeField] List<TilePosition> startPositions;

    public int burstIndex = 0;
    public bool burst;
    public int burstCount = 10;

    void Start()
    {
        Trail[] existingTrails = GetComponentsInChildren<Trail>();
        foreach (Trail trail in existingTrails)
        {
            DestroyImmediate(trail.gameObject);
        }
    }

    void Update()
    {

        if (trailPrefab == null || tileController == null) return;

        if (continuousSpawn)
        {
            if (Time.time - timeAtLastSpawn >= 1.0f / spawnRate)
            {
                for (int i = 0; i < trailsPerSpawn; i++)
                {
                    SpawnTrail(continuousSpawnIndex);
                }

                timeAtLastSpawn = Time.time;
            }
        }

        if (burst)
        {
            for (int i = 0; i < burstCount; i++)
            {
                SpawnTrail(burstIndex);
            }
            burst = false;
        }

        Trail[] trails = GetComponentsInChildren<Trail>();
        foreach (Trail trail in trails)
        {
            trail.trailLength = trailLength;
            trail.trailWidth = trailWidth;
            trail.maxLife = trailLife - trail.uniqueRandomness * trailLife;
            trail.speed = trailSpeed;
            trail.speedRandomness = trailSpeedRandomness;
            trail.escapeDirections = trailEscapeDirections.ToArray();
            trail.escapeProbability = trailEscapeProbability;
        }

        if (clearTrails)
        {
            foreach (Trail trail in trails)
            {
                DestroyImmediate(trail.gameObject);
            }
            clearTrails = false;
        }

    }

    void SpawnTrail(int forcePosIndex = -1)
    {
        if (startPositions.Count == 0) return;
        TilePosition startPos = forcePosIndex >= 0 ? startPositions[forcePosIndex % startPositions.Count] : getRandomPosition();

        Vector3 pos = tileController.getPositionAt(startPos.x, startPos.y, true, offset);
        GameObject newTrail = Instantiate(trailPrefab, pos, Quaternion.identity);
        newTrail.transform.SetParent(transform, true);

        Trail trailComponent = newTrail.GetComponent<Trail>();
        if (trailComponent != null)
        {
            trailComponent.Init(tileController, startPos.x, startPos.y, offset);
        }
    }

    TilePosition getRandomPosition()
    {
        TilePosition result = new TilePosition();
        result.x = Random.Range(0, tileController.horizontalCount);
        result.y = Random.Range(0, tileController.verticalCount);
        return result;
    }
}
