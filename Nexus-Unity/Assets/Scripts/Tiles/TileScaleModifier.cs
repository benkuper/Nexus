using UnityEngine;

[ExecuteAlways]
public class TileScaleModifier : TileModifier
{
    
    [Header("Scale Settings")]
    [Range(0f, 1f)]
    public float outsideScale = 0.1f;
    [Range(0f, 1f)]

    public float insideScale = 1f;


    public override void updateTile(Tile tile, float weight)
    {
        float scale = Mathf.Lerp(outsideScale, insideScale, weight);
        tile.transform.localScale *= scale;
    }
}
