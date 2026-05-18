using UnityEngine;

[ExecuteInEditMode]
public class TileModifierInfluencer : MonoBehaviour
{
    [Range(0f, 1f)]
    public float weight = 1f;
    [Range(0f, 1f)]
    public float randomness = 0f;
    public float randomScale = 1f;
    public float randomSpeed = 0f;

    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    float curTime = 0f;


    void Update()
    {
        curTime += Time.deltaTime * randomSpeed;
    }

    virtual public float getWeightForTile(Tile tile)
    {
        Vector3 pos = tile.transform.position;
        float r = getRandomnessAtPos(pos);
        return weight * animationCurve.Evaluate(r);
    }

    virtual public float getRandomnessAtPos(Vector3 pos)
    {
        float noise = randomness > 0f ? Mathf.PerlinNoise((pos.x + pos.z) * randomScale + curTime, (pos.y + pos.z) * randomScale + curTime) : 0f;
        return noise * randomness;
    }
}