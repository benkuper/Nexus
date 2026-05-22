using UnityEngine;

[ExecuteAlways]
public class TileMoveModifier : TileModifier
{
    [Header("Motion Settings")]
    public Vector3 motionAmplitude;
    public float motionFrequency = 1f;
    [Range(0f, 1f)] public float motionTileRandomness = 0.5f;
    public float motionTileRandomScale = 0.5f;

    [Header("Rotation Settings")]
    public Vector3 rotationAmplitude;
    public float rotationFrequency = 1f;
    [Range(0f, 1f)] public float rotationTileRandomness = 0.5f;
    public float rotationTileRandomScale = 0.5f;

    public override void updateTile(Tile tile, float weight)
    {
        Vector3 motionNoise = SampleNoiseVector3(tile, motionFrequency, motionTileRandomScale, motionTileRandomness, 0f);
        Vector3 motionOffset = Vector3.Scale(motionNoise, motionAmplitude) * weight;
        tile.transform.localPosition += motionOffset;

        Vector3 rotationNoise = SampleNoiseVector3(tile, rotationFrequency, rotationTileRandomScale, rotationTileRandomness, 100f);
        Vector3 rotationOffset = Vector3.Scale(rotationNoise, rotationAmplitude) * weight;
        tile.transform.localRotation *= Quaternion.Euler(rotationOffset);
    }

    private static Vector3 SampleNoiseVector3(Tile tile, float frequency, float scale, float randomness, float seed)
    {
        float time = Time.unscaledTime * frequency;
        float x = tile.x * scale * randomness;
        float y = tile.y * scale * randomness;

        return new Vector3(
            SampleSignedNoise3D(x + 11.31f + seed, y + 17.29f, time + 23.73f),
            SampleSignedNoise3D(x + 31.97f + seed, y + 47.11f, time + 53.59f),
            SampleSignedNoise3D(x + 61.43f + seed, y + 71.83f, time + 83.67f)
        );
    }

    private static float SampleSignedNoise3D(float x, float y, float z)
    {
        return SampleNoise3D(x, y, z) * 2f - 1f;
    }

    private static float SampleNoise3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float yz = Mathf.PerlinNoise(y, z);
        float xz = Mathf.PerlinNoise(x, z);
        float yx = Mathf.PerlinNoise(y, x);
        float zy = Mathf.PerlinNoise(z, y);
        float zx = Mathf.PerlinNoise(z, x);
        return (xy + yz + xz + yx + zy + zx) / 6f;
    }
}
