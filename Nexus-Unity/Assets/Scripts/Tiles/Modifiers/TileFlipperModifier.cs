using UnityEngine;

public class TileFlipperModifier : TileModifier
{
    [Range(0f, 1f)]
    public float progression = 0f; // 0 = no flip, 1 = fully flipped
    [Range(0f, 1f)]
    public float progressionFade = 0.5f; // Controls how wide the sweep affects neighboring tiles
    [Range(0f, 1f)]
    public float flipRandomness = 0.5f; // Controls how different tiles are from each other (1 = max variation between tiles)

    public float flipAmount = 180;

    public bool verticalProgression;
    public bool reverseProgression;
    public bool reverseFlip;
    public bool verticalFlip;


    public Color color1 = Color.white;
    public Color color2 = Color.red;

    [Range(0f, 1f)]
    public float texWeight1 = 1f;
    [Range(0f, 1f)]
    public float texWeight2 = 1f;

    [Range(0f, 1f)]
    public float lineHWeight1 = 1f;
    [Range(0f, 1f)]
    public float lineHWeight2 = 1f;
    [Range(0f, 1f)]
    public float lineVWeight1 = 1f;
    [Range(0f, 1f)]
    public float lineVWeight2 = 1f;

    MaterialPropertyBlock materialBlock;

    public override void updateTile(Tile tile, float weight)
    {
        float eProg = Mathf.Lerp(-progressionFade - flipRandomness, 1 + progressionFade + flipRandomness, progression);
        float tileProg = verticalProgression ? tile.relativeY : tile.relativeX;
        float randomOffset = Mathf.PerlinNoise(tile.x * 13.7f, tile.y * 17.3f) * flipRandomness;
        tileProg += randomOffset;
        if (reverseProgression)
        {
            tileProg = 1f - tileProg;
        }

        float progDiff = tileProg - eProg;
        float flipProg = Mathf.Clamp01(1f - progDiff / Mathf.Max(progressionFade, 0.0001f));
        if (reverseFlip)
        {
            flipProg = 1f - flipProg;
        }

        float flipAngle = flipProg * flipAmount * weight;
        if (verticalFlip)
        {
            tile.transform.localRotation *= Quaternion.Euler(flipAngle, 0f, 0f);
        }
        else
        {
            tile.transform.localRotation *= Quaternion.Euler(0f, flipAngle, 0f);
        }

        Color tileColor = Color.Lerp(color1, color2, flipProg * weight);

        Renderer renderer = tile.GetComponentInChildren<Renderer>();
        Material sharedMaterial = renderer.sharedMaterial;


        if (sharedMaterial == null)
        {
            return;
        }
        if (materialBlock == null)
        {
            materialBlock = new MaterialPropertyBlock();
        }
        renderer.GetPropertyBlock(materialBlock);

      
        if (sharedMaterial.HasProperty(TileController.BaseColorPropertyId))
        {
            materialBlock.SetColor(TileController.BaseColorPropertyId, tileColor);
        }


        renderer.SetPropertyBlock(materialBlock);

    }
}