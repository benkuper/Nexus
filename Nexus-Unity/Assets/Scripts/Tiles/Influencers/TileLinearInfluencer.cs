using UnityEngine;

public class TileLinearInfluencer : TileModifierInfluencer
{
    public TileController[] controllers;


    [Range(0f, 1f)]
    public float origin = 0f;

    public float progression = 0f;
    [Range(0f, 1f)]
    public float borderRadius = 0f;

    override public float getWeightForTile(Tile tile)
    {
        TileController controller = tile.GetComponentInParent<TileController>();
        int groupIndex = System.Array.IndexOf(controllers, controller);
        if (groupIndex == -1) return 0f;

        float groupX = groupIndex / (float)controllers.Length;
        float localX = tile.x / (float)controller.horizontalCount; 

        float absX = groupX + localX / controllers.Length + getRandomnessAtPos(tile.transform.position) - .5f * randomness;
        float dist = Mathf.Abs(absX - origin);

        float target = 1f - Mathf.Clamp01((dist - progression) / borderRadius);
        return animationCurve.Evaluate(target) * weight;
    }
}
