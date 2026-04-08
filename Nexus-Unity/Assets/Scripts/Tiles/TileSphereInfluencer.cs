using UnityEngine;

public class TileSphereInfluencer : TileModifierInfluencer
{

    public override float getWeightAtPos(Vector3 pos)
    {
        Vector3 localPos = transform.InverseTransformPoint(pos);
        
        float noiseXY = randomness > 0f ? Mathf.PerlinNoise(localPos.magnitude * randomScale, localPos.magnitude* 1.37f * randomScale) : 0f;
        float curveValue = animationCurve.Evaluate(localPos.magnitude * 2 + Mathf.Lerp(0f, noiseXY, randomness));
        return weight * curveValue;
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
    }
}
