using UnityEngine;

[ExecuteAlways]
public class TileMoveModifier : TileModifier
{
    
    [Header("Motion Settings")]
    public Vector3 motionAmplitude;
    public float motionFrequency = 1f;
    public float motionSpeed = 1f;
    public float motionAmountRandomness = 0.5f;
    public float motionSpeedRandomness = 0.5f;


    [Header("Rotation Settings")]
    public Vector3 rotationAmplitude;
    public float rotationFrequency = 1f;
    public float rotationSpeed = 1f;
    public float rotationAmountRandomness = 0.5f;
    public float rotationSpeedRandomness = 0.5f;


    protected override void Update()
    {
        updateTiles();
    }

    public override void updateTile(Transform tile, float weight)
    {
        Debug.Log("Updating tile: " + tile.name + " with weight: " + weight);
        float time = Time.time * motionSpeed * (1f + (Random.value - 0.5f) * motionSpeedRandomness);
        Vector3 motionOffset = new Vector3(
            Mathf.Sin(time * motionFrequency) * motionAmplitude.x * weight * (1f + (Random.value - 0.5f) * motionAmountRandomness),
            Mathf.Sin(time * motionFrequency) * motionAmplitude.y * weight * (1f + (Random.value - 0.5f) * motionAmountRandomness),
            Mathf.Sin(time * motionFrequency) * motionAmplitude.z * weight * (1f + (Random.value - 0.5f) * motionAmountRandomness)
        );

        Vector3 rotationOffset = new Vector3(
            Mathf.Sin(time * rotationFrequency) * rotationAmplitude.x * weight * (1f + (Random.value - 0.5f) * rotationAmountRandomness),
            Mathf.Sin(time * rotationFrequency) * rotationAmplitude.y * weight * (1f + (Random.value - 0.5f) * rotationAmountRandomness),
            Mathf.Sin(time * rotationFrequency) * rotationAmplitude.z * weight * (1f + (Random.value - 0.5f) * rotationAmountRandomness)
        );

        tile.localPosition += motionOffset;
        tile.localRotation *= Quaternion.Euler(rotationOffset);
    }
}
