using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[OSCQuery.DoNotExposeChildren]
[ExecuteAlways]
public class TileController : MonoBehaviour
{
    public static readonly int BaseColorPropertyId = Shader.PropertyToID("_Base_Color");
    public static readonly int MainTexStPropertyId = Shader.PropertyToID("_Texture_ST");

    public static readonly int BorderWidthPropertyId = Shader.PropertyToID("_Border_Width");
    public static readonly int BorderColorPropertyId = Shader.PropertyToID("_Border_Color");
    public static readonly int BorderIntensityPropertyId = Shader.PropertyToID("_Border_Intensity");
    public static readonly int HorizontalBorderPropertyId = Shader.PropertyToID("_Horizontal_Border");
    public static readonly int VerticalBorderPropertyId = Shader.PropertyToID("_Vertical_Border");

    public static readonly int MetallicPropertyId = Shader.PropertyToID("_Metallic");
    public static readonly int SmoothnessPropertyId = Shader.PropertyToID("_Smoothness");
    public static readonly int TextureWeightPropertyId = Shader.PropertyToID("_Texture_Alpha");


    [Header("Grid Bounds")]
    [Min(0f)] public float totalWidth = 10f;
    [Min(0f)] public float totalHeight = 10f;
    [Min(0f)] public float totalAllWallsWidth = 1f;
    [Min(0f)] public float wallOffset = 0.5f;
    [Min(0f)] public float relativeHeight = 1f;

    [Header("Grid Count")]
    [Min(0)][SerializeField] private int horizontalCount = 5;
    [Min(0)][SerializeField] private int verticalCount = 5;

    [Header("Layout")]
    [Range(0f, 1f)]
    [SerializeField] private float spread = 0f;
    [SerializeField] private bool centerGrid = true;

    [Header("Tile")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Vector3 tileScale = Vector3.one;
    [Min(0f)][SerializeField] private float tileDepth = 0.1f;
    [SerializeField] private string tileName = "Tile";

    [Header("Modifiers")]
    public GameObject globalModifiers;

    [Header("Material Settings")]
    [SerializeField] private Color baseColor = Color.black;
    [SerializeField] private Color borderColor = Color.black;
    [Range(0f, 1)][SerializeField] private float metallic = 0f;
    [Range(0f, 1)][SerializeField] private float smoothness = 0f;
    [Range(0f, 0.99f)][SerializeField] private float borderWidth = 0f;
    [Range(0f, 1)][SerializeField] private float borderIntensity = 1f;
    [Range(0f, 1f)][SerializeField] private float horizontally = 0f;
    [Range(0f, 1f)][SerializeField] private float vertically = 0f;
    [Range(0f, 1f)][SerializeField] private float textureWeight = 1f;

    [Header("Live Update")]
    [SerializeField] private bool autoRefresh = true;

    [SerializeField, HideInInspector] private Transform tilesContainer;
    [SerializeField, HideInInspector] private List<Tile> generatedTiles = new List<Tile>();
    [SerializeField, HideInInspector] private int cachedTileCount = -1;
    [SerializeField, HideInInspector] private GameObject cachedPrefab;

    private MaterialPropertyBlock materialBlock;

    private void OnEnable()
    {
        if (materialBlock == null)
        {
            materialBlock = new MaterialPropertyBlock();
        }

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
        if (autoRefresh)
        {
            LayoutTiles();
        }
        else
        {
            ApplyModifiers();
        }
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
        spread = Mathf.Max(0f, spread);
        tileDepth = Mathf.Max(0f, tileDepth);
        borderWidth = Mathf.Clamp01(borderWidth);
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

        float tileWidth = GetTileSize(totalWidth, horizontalCount, spread);
        float tileHeight = GetTileSize(totalHeight, verticalCount, spread);
        float strideX = tileWidth + spread;
        float strideY = tileHeight + spread;
        float startX = centerGrid ? -((horizontalCount - 1) * strideX) * 0.5f : tileWidth * 0.5f;
        float startY = centerGrid ? -((verticalCount - 1) * strideY) * 0.5f : tileHeight * 0.5f;
        Vector3 resolvedTileScale = new Vector3(
            tileWidth,
            tileHeight,
            tileDepth
        );
        int tileIndex = 0;

        for (int y = 0; y < verticalCount; y++)
        {
            for (int x = 0; x < horizontalCount; x++)
            {
                Tile tile = generatedTiles[tileIndex];
                if (tile == null) continue;
                tile.x = x;
                tile.y = y;
                tile.relativeX = horizontalCount > 1 ? (float)x / (horizontalCount - 1) : 0f;
                tile.relativeY = verticalCount > 1 ? (float)y / (verticalCount - 1) : 0f;
                tile.name = $"{tileName} {tileIndex + 1}";
                tile.transform.localPosition = new Vector3(startX + (x * strideX), startY + (y * strideY), 0f);
                tile.transform.localRotation = Quaternion.identity;
                tile.transform.localScale = resolvedTileScale;


                // set material uv tiling and offset based on grid size
                Renderer renderer = tile.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = true;
                    ApplyTileMaterial(renderer, x, y);
                }

                tileIndex++;

            }
        }

        ApplyModifiers();

    }

    public Vector3 getPositionAt(int x, int y, bool returnCorner = true, float offset = 0f)
    {
        float tileWidth = GetTileSize(totalWidth, horizontalCount, spread);
        float tileHeight = GetTileSize(totalHeight, verticalCount, spread);
        float strideX = tileWidth + spread;
        float strideY = tileHeight + spread;
        float startX = centerGrid ? -((horizontalCount - 1) * strideX) * 0.5f : tileWidth * 0.5f;
        float startY = centerGrid ? -((verticalCount - 1) * strideY) * 0.5f : tileHeight * 0.5f;
        if (returnCorner)
        {
            startX -= tileWidth * 0.5f;
            startY -= tileHeight * 0.5f;
        }
        return transform.TransformPoint(new Vector3(startX + (x * strideX), startY + (y * strideY), 0f) + Vector3.forward * offset);
    }

    private void ApplyTileMaterial(Renderer renderer, int x, int y)
    {
        if (materialBlock == null)
        {
            materialBlock = new MaterialPropertyBlock();
        }

        Material sharedMaterial = renderer.sharedMaterial;

        if (sharedMaterial == null)
        {
            return;
        }

        renderer.GetPropertyBlock(materialBlock);

        float wallRelativeWidth = totalWidth / totalAllWallsWidth;
        float wallRelativeOffsetX = wallOffset / totalAllWallsWidth;
        float wallRelativeOffsetY = 1 - relativeHeight;

        Vector4 textureSt = new Vector4(
            -wallRelativeWidth / Mathf.Max(1, horizontalCount),
            -relativeHeight / Mathf.Max(1, verticalCount),
            wallRelativeOffsetX + ((float)(x+1) / Mathf.Max(1, horizontalCount)) * wallRelativeWidth,
            wallRelativeOffsetY + ((float)(y+1) / Mathf.Max(1, verticalCount)) * relativeHeight
        );

        if (sharedMaterial.HasProperty(MainTexStPropertyId))
        {
            materialBlock.SetVector(MainTexStPropertyId, textureSt);
        }

        if (sharedMaterial.HasProperty(BaseColorPropertyId))
        {
            materialBlock.SetColor(BaseColorPropertyId, baseColor);
        }

        if (sharedMaterial.HasProperty(BorderColorPropertyId))
        {
            materialBlock.SetColor(BorderColorPropertyId, borderColor);
        }

        if (sharedMaterial.HasProperty(BorderIntensityPropertyId))
        {
            materialBlock.SetFloat(BorderIntensityPropertyId, borderIntensity);
        }

        if (sharedMaterial.HasProperty(BorderWidthPropertyId))
        {
            materialBlock.SetFloat(BorderWidthPropertyId, borderWidth);
        }

        if (sharedMaterial.HasProperty(MetallicPropertyId))
        {
            materialBlock.SetFloat(MetallicPropertyId, metallic);
        }

        if (sharedMaterial.HasProperty(SmoothnessPropertyId))
        {
            materialBlock.SetFloat(SmoothnessPropertyId, smoothness);
        }

        if (sharedMaterial.HasProperty(HorizontalBorderPropertyId))
        {
            materialBlock.SetFloat(HorizontalBorderPropertyId, horizontally);
        }

        if (sharedMaterial.HasProperty(VerticalBorderPropertyId))
        {
            materialBlock.SetFloat(VerticalBorderPropertyId, vertically);
        }

        if( sharedMaterial.HasProperty(TextureWeightPropertyId))
        {
            materialBlock.SetFloat(TextureWeightPropertyId, textureWeight);
        }


        renderer.SetPropertyBlock(materialBlock);
    }

    void ApplyModifiers()
    {
        TileModifier[] modifiers = GetComponents<TileModifier>();

        Tile[] tiles = generatedTiles.ToArray();
        foreach (TileModifier modifier in modifiers)
        {
            if (!modifier.enabled) continue;
            modifier.updateTiles(tiles);
        }

        if (globalModifiers != null)
        {
            TileModifier[] globalMods = globalModifiers.GetComponents<TileModifier>();
            foreach (TileModifier modifier in globalMods)
            {
                if (!modifier.enabled) continue;
                modifier.updateTiles(tiles);
            }
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

    public bool IsValidTile(int targetX, int targetY)
    {
        return targetX >= 0 && targetX < horizontalCount && targetY >= 0 && targetY < verticalCount;
    }
}
