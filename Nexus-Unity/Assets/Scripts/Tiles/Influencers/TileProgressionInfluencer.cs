using UnityEngine;

public class TileProgressionInfluencer : TileModifierInfluencer
{
    public float progression = 0f;

    enum ProgressionDirection
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }

    [SerializeField] ProgressionDirection progressionDirection = ProgressionDirection.LeftToRight;

    public override float getWeightForTile(Tile tile)
    {
        Vector3 pos = tile.transform.position;
        Vector3 localPos = transform.InverseTransformPoint(pos);

        float noise = getRandomnessAtPos(pos);


        float relativePos = 0f;


        switch (progressionDirection)
        {
            case ProgressionDirection.LeftToRight:
                relativePos = 1- progression - localPos.x;
                break;

            case ProgressionDirection.RightToLeft:
                relativePos = (localPos.x - progression);
                break;

            case ProgressionDirection.TopToBottom:
                relativePos = localPos.y - progression;
                break;

            case ProgressionDirection.BottomToTop:
                relativePos = 1 - (localPos.y - progression);
                break;

        }


        float curveValue = animationCurve.Evaluate(relativePos + noise);
        return weight * curveValue;
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
