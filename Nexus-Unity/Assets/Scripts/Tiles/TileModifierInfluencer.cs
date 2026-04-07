using UnityEngine;

public class TileModifierInfluencer : MonoBehaviour
{
    [Range(0f, 1f)]
    public float weight = 1f;

    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

 
    virtual public float getWeightAtPos(Vector3 pos)
    {
        return weight;
    }
}
