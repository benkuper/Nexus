using UnityEngine;

public class TileSphereInfluencer : TileModifierInfluencer
{

    public override float getWeightAtPos(Vector3 pos)
    {
        Vector3 localPos = transform.InverseTransformPoint(pos);
        Vector3 normalizedPos = new Vector3(
            Mathf.InverseLerp(-0.5f, 0.5f, localPos.x),
            Mathf.InverseLerp(-0.5f, 0.5f, localPos.y),
            Mathf.InverseLerp(-0.5f, 0.5f, localPos.z)
        );

        float curveValue = animationCurve.Evaluate((normalizedPos.x + normalizedPos.y + normalizedPos.z) / 3f);
        return weight * curveValue;
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
    }
}
