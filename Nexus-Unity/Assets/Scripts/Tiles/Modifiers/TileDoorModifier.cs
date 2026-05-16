using UnityEngine;

[ExecuteAlways]
public class TileDoorModifier : TileModifier
{

    public Vector3 posLeft;
    public Vector3 posRight;

    public override void updateTile(Tile tile, float weight)
    {
        bool isLeft = tile.relativeX < .5f;
        float angle = weight * 90;
        if (isLeft) angle = -angle;
        tile.transform.RotateAround(isLeft ? posLeft : posRight, Vector3.up, angle);
        Vector3 targetPos = transform.position;
        Quaternion tartRot = Quaternion.identity;


    }
}
