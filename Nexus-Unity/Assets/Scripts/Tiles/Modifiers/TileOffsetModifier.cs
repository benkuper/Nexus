using UnityEngine;

[ExecuteAlways]
public class TileOffsetModifier : TileModifier
{
    public Vector3 offset;
    public Transform repelOrigin;
    [Range(0f, 10f)]
    public float repel;
    [Range(.01f, 100f)]
    public float repelScale;


    public override void updateTile(Tile tile, float weight)
    {
        base.updateTile(tile, weight);

        Vector3 target = Vector3.zero;


        if (offset != Vector3.zero)
        {
            target = offset * weight;

        }


        if(repelOrigin != null && repel > 0)
        {
            Vector3 repelDir = tile.transform.position - repelOrigin.position;
            float repelDist = repelDir.magnitude * repelScale;
            if (repelDist > 0)
            {
                repelDir /= repelDist;
                target += tile.transform.TransformPoint(repelDir) * repel * weight;
            }
        }

        tile.transform.localPosition += target;
    }
}
