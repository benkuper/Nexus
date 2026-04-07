using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class TileController : MonoBehaviour
{
    [Header("Grid Bounds")]
    [Min(0f)] public float totalWidth = 10f;
    [Min(0f)] public float totalHeight = 10f;

    [Header("Grid Count")]
    [Min(0)][SerializeField] private int horizontalCount = 5;
    [Min(0)][SerializeField] private int verticalCount = 5;

    [Header("Layout")]
    [SerializeField] private Vector2 spread = new Vector2(0.1f, 0.1f);
    [SerializeField] private bool centerGrid = true;

    [Header("Tile")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Vector3 tileScale = Vector3.one;
    [Min(0f)][SerializeField] private float tileDepth = 0.1f;
    [SerializeField] private string tileName = "Tile";

    [Header("Live Update")]
    [SerializeField] private bool autoRefresh = true;

    [SerializeField, HideInInspector] private Transform tilesContainer;
    [SerializeField, HideInInspector] private List<Tile> generatedTiles = new List<Tile>();
    [SerializeField, HideInInspector] private int cachedTileCount = -1;
    [SerializeField, HideInInspector] private GameObject cachedPrefab;

    private void OnEnable()
    {
        if (autoRefresh)
        {
            RefreshTiles();
        }
    }

    private void OnValidate()
    {
        ClampSettings();

        if (!autoRefresh)
        {
            return;
        }

        int targetCount = horizontalCount * verticalCount;
        bool countChanged = targetCount != cachedTileCount;
        bool prefabChanged = tilePrefab != cachedPrefab;

        if (countChanged || prefabChanged)
        {
            RefreshTiles();
        }
        else
        {
            LayoutTiles();
        }
    }

    void Update()
    {
        LayoutTiles();
    }

    [ContextMenu("Refresh Tiles")]
    public void RefreshTiles()
    {
        ClampSettings();
        ClearGeneratedTiles();
        EnsureTileCount();
        LayoutTiles();
        cachedTileCount = generatedTiles.Count;
        cachedPrefab = tilePrefab;
    }

    [ContextMenu("Clear Generated Tiles")]
    public void ClearGeneratedTiles()
    {
        RebuildTileCache();

        for (int index = generatedTiles.Count - 1; index >= 0; index--)
        {
            DestroyTile(generatedTiles[index]);
        }

        generatedTiles.Clear();
        cachedTileCount = 0;
    }

    private Transform GetOrCreateContainer()
    {
        if (tilesContainer != null)
        {
            return tilesContainer;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name == "TilesContainer")
            {
                tilesContainer = transform.GetChild(i);
                return tilesContainer;
            }
        }

        GameObject container = new GameObject("TilesContainer");
        container.transform.SetParent(transform, false);
        tilesContainer = container.transform;
        return tilesContainer;
    }

    private void ClampSettings()
    {
        horizontalCount = Mathf.Max(0, horizontalCount);
        verticalCount = Mathf.Max(0, verticalCount);
        spread.x = Mathf.Max(0f, spread.x);
        spread.y = Mathf.Max(0f, spread.y);
        tileDepth = Mathf.Max(0f, tileDepth);
        tileScale.x = Mathf.Max(0f, tileScale.x);
        tileScale.y = Mathf.Max(0f, tileScale.y);
        tileScale.z = Mathf.Max(0f, tileScale.z);
    }

    private void RebuildTileCache()
    {
        if (generatedTiles == null)
        {
            generatedTiles = new List<Tile>();
        }

        generatedTiles.RemoveAll(tile => tile == null);

        if (tilesContainer == null)
        {
            return;
        }

        for (int childIndex = 0; childIndex < tilesContainer.childCount; childIndex++)
        {
            Transform child = tilesContainer.GetChild(childIndex);

            if (child.TryGetComponent(out Tile tile) && !generatedTiles.Contains(tile))
            {
                generatedTiles.Add(tile);
            }
        }
    }

    private void EnsureTileCount()
    {
        int targetCount = horizontalCount * verticalCount;

        while (generatedTiles.Count < targetCount)
        {
            generatedTiles.Add(CreateTile(generatedTiles.Count));
        }

        while (generatedTiles.Count > targetCount)
        {
            int lastIndex = generatedTiles.Count - 1;
            Tile tile = generatedTiles[lastIndex];
            generatedTiles.RemoveAt(lastIndex);
            DestroyTile(tile);
        }
    }

    private Tile CreateTile(int index)
    {
        Transform container = GetOrCreateContainer();
        GameObject tileObject;

#if UNITY_EDITOR
        if (!Application.isPlaying && tilePrefab != null)
        {
            tileObject = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab, container);
        }
        else
#endif
            if (tilePrefab != null)
            {
                tileObject = Instantiate(tilePrefab, container);
            }
            else
            {
                tileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tileObject.transform.SetParent(container, false);
            }

        tileObject.name = $"{tileName} {index + 1}";
        tileObject.transform.localRotation = Quaternion.identity;

        if (!tileObject.TryGetComponent(out Tile _))
        {
            tileObject.AddComponent<Tile>();
        }

        return tileObject.GetComponent<Tile>();
    }

    private void LayoutTiles()
    {
        if (generatedTiles.Count == 0 || horizontalCount == 0 || verticalCount == 0)
        {
            return;
        }

        float tileWidth = GetTileSize(totalWidth, horizontalCount, spread.x);
        float tileHeight = GetTileSize(totalHeight, verticalCount, spread.y);
        float strideX = tileWidth + spread.x;
        float strideY = tileHeight + spread.y;
        float startX = centerGrid ? -((horizontalCount - 1) * strideX) * 0.5f : tileWidth * 0.5f;
        float startY = centerGrid ? -((verticalCount - 1) * strideY) * 0.5f : tileHeight * 0.5f;
        Vector3 resolvedTileScale = new Vector3(tileWidth * tileScale.x, tileHeight * tileScale.y, tileDepth * tileScale.z);
        int tileIndex = 0;

        for (int y = 0; y < verticalCount; y++)
        {
            for (int x = 0; x < horizontalCount; x++)
            {
                Tile tile = generatedTiles[tileIndex];
                if(tile == null) continue;
                tile.x = x;
                tile.y = y;
                tile.relativeX = horizontalCount > 1 ? (float)x / (horizontalCount - 1) : 0f;
                tile.relativeY = verticalCount > 1 ? (float)y / (verticalCount - 1) : 0f;
                tile.name = $"{tileName} {tileIndex + 1}";
                tile.transform.localPosition = new Vector3(startX + (x * strideX), startY + (y * strideY), 0f);
                tile.transform.localRotation = Quaternion.identity;
                tile.transform.localScale = resolvedTileScale;
                tileIndex++;

            }
        }

        ApplyModifiers();

    }

    void ApplyModifiers()
    {
        TileModifier[] modifiers = GetComponents<TileModifier>();

        foreach (TileModifier modifier in modifiers)
        {
            if(!modifier.enabled) continue;
            modifier.updateTiles();
        }
    }

    private static float GetTileSize(float totalSize, int count, float spacing)
    {
        if (count <= 0)
        {
            return 0f;
        }

        float totalSpacing = Mathf.Max(0, count - 1) * spacing;
        return Mathf.Max(0f, (totalSize - totalSpacing) / count);
    }

    private void DestroyTile(Tile tile)
    {
        if (tile == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(tile.gameObject);
            return;
        }

        DestroyImmediate(tile.gameObject);
    }
}
