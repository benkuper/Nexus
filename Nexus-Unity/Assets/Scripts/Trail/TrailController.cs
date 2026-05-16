using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TrailController : MonoBehaviour
{
    public GameObject trailPrefab;
    public TileController tileController;

    public float spawnRate;
    float timeAtLastSpawn = 0f;

    public bool continuousSpawn;

    public float offset = 0f;

    [Header("Trail Rendering")]
    public float trailWidth = 1f;
    public float trailLength = 1f;
    public int trailSteps = 50;
    [Range(0,1)]
    public float trailSpeed = .1f;
    [Range(0f, 1f)]
    public float trailSpeedRandomness = 0f;
    public bool clearTrails;

    [System.Serializable]
    public struct TilePosition
    {
        public int x, y;
    }

    [SerializeField] List<TilePosition> startPositions;

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
                SpawnTrail();
                timeAtLastSpawn = Time.time;
            }
        }

        if(burst)
        {
            int posIndex = Random.Range(0, startPositions.Count);
            for (int i = 0; i < burstCount; i++)
            {
                SpawnTrail(posIndex);
            }
            burst = false;
        }

        Trail[] trails = GetComponentsInChildren<Trail>();
        foreach(Trail trail in trails)
        {
            trail.trailLength = trailLength;
            trail.trailWidth = trailWidth;
            trail.maxSteps = trailSteps;
            trail.speed = trailSpeed;
            trail.speedRandomness = trailSpeedRandomness;
        }

        if(clearTrails)
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
        if(startPositions.Count == 0) return;
        TilePosition startPos = startPositions[forcePosIndex >= 0 ? forcePosIndex % startPositions.Count : Random.Range(0, startPositions.Count)];

        Vector3 pos = tileController.getPositionAt(startPos.x, startPos.y, true, offset);
        GameObject newTrail = Instantiate(trailPrefab, pos, Quaternion.identity);
        newTrail.transform.SetParent(transform, true);

        Trail trailComponent = newTrail.GetComponent<Trail>();
        if (trailComponent != null)
        {
            trailComponent.Init(tileController, startPos.x, startPos.y, offset);
        }
    }
}
