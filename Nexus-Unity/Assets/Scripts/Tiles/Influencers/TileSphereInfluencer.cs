using UnityEngine;

public class TileSphereInfluencer : TileModifierInfluencer
{

    public override float getWeightAtPos(Vector3 pos)
    {
        Vector3 localPos = transform.InverseTransformPoint(pos);
        
        float noise = getRandomnessAtPos(pos);
        float curveValue = animationCurve.Evaluate(localPos.magnitude + noise);
        return weight * curveValue;
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
    }
}
