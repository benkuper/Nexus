using UnityEngine;

public class TileAxialInfluencer : TileModifierInfluencer
{
    public override float getWeightAtPos(Vector3 pos)
    {
        Vector3 localPos = transform.InverseTransformPoint(pos);

        float noise = getRandomnessAtPos(pos);
        Vector2 localPosXZ = new Vector2(localPos.x, localPos.z);
        float curveValue = animationCurve.Evaluate(localPosXZ.magnitude + noise);
        return weight * curveValue;
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Vector3.up*transform.lossyScale.y, Vector3.down*transform.lossyScale.y);
    }
}
