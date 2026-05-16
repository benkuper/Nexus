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
