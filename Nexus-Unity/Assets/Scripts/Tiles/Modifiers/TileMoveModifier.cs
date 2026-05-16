using UnityEngine;

[ExecuteAlways]
public class TileMoveModifier : TileModifier
{
    [Header("Motion Settings")]
    public Vector3 motionAmplitude;
    public float motionFrequency = 1f;
    [Range(0f, 1f)] public float motionTileRandomness = 0.5f;

    [Header("Rotation Settings")]
    public Vector3 rotationAmplitude;
    public float rotationFrequency = 1f;
    [Range(0f, 1f)] public float rotationTileRandomness = 0.5f;

    public override void updateTile(Tile tile, float weight)
    {

        // Controls how different tiles are from each other (1 = max variation between tiles)
        float motionTileAmount = Mathf.PerlinNoise(tile.x * motionTileRandomness, tile.y * motionTileRandomness);
        float rotTileAmount = Mathf.PerlinNoise(tile.x * rotationTileRandomness, tile.y * rotationTileRandomness);

        float mt = Time.time * motionFrequency + tile.x * motionTileRandomness * tile.y;
        float rt = Time.time * rotationFrequency + tile.x * rotationTileRandomness * tile.y;

        Vector3 motion = new Vector3(
            (Mathf.PerlinNoise(mt + motionTileRandomness, 0f) * 2f - 1f) * motionAmplitude.x,
            (Mathf.PerlinNoise(mt + motionTileRandomness, 1f) * 2f - 1f) * motionAmplitude.y,
            (Mathf.PerlinNoise(mt + motionTileRandomness, 2f) * 2f - 1f) * motionAmplitude.z
        );

        Vector3 rotation = new Vector3(
            (Mathf.PerlinNoise(rt + rotationTileRandomness + 10f, 0f) * 2f - 1f) * rotationAmplitude.x,
            (Mathf.PerlinNoise(rt + rotationTileRandomness + 10f, 1f) * 2f - 1f) * rotationAmplitude.y,
            (Mathf.PerlinNoise(rt + rotationTileRandomness + 10f, 2f) * 2f - 1f) * rotationAmplitude.z
        );

        tile.transform.localPosition += motion * motionTileAmount * weight;
        tile.transform.localRotation *= Quaternion.Euler(rotation * rotTileAmount * weight);
    }
}
