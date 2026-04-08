using UnityEngine;

public class TileModifierInfluencer : MonoBehaviour
{
    [Range(0f, 1f)]
    public float weight = 1f;
    [Range(0f, 1f)]
    public float randomness = 0f;
    public float randomScale = 1f;

    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);


    virtual public float getWeightAtPos(Vector3 pos)
    {
        return weight * animationCurve.Evaluate(randomness > 0f ? Mathf.PerlinNoise(pos.x * randomness, pos.y * randomness) : 0f);
    }
}
