using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TileModifier : MonoBehaviour
{
    public List<TileModifierInfluencer> influencers = new List<TileModifierInfluencer>();

    Transform tileContainer;

    [Range(0f, 1f)]
    public float modifierWeight = 1f;

    [Range(0f, 1f)]
    public float influencerWeight = 1f;

    void OnEnable()
    {
        tileContainer = transform.Find("TilesContainer");
    }

    public enum WeightMode
    {
        Average,
        Multiply,
        Max
    }
    public WeightMode weightMode = WeightMode.Max;

    // Update is called once per frame
    virtual protected void Update()
    {
    }

    virtual public void updateTiles(Tile[] tiles)
    {
        foreach (Tile tile in tiles)
        {
            int goodInfluencers = 0;
            float totalWeight = weightMode == WeightMode.Multiply ? 1f : 0f;
            foreach (TileModifierInfluencer influencer in influencers)
            {
                if (influencer == null || !influencer.isActiveAndEnabled)
                {
                    continue;
                }
                goodInfluencers++;

                float w = influencer.getWeightForTile(tile);
                switch (weightMode)
                {
                    case WeightMode.Average:
                        totalWeight += w;
                        break;
                    case WeightMode.Multiply:
                        totalWeight *= w;
                        break;
                    case WeightMode.Max:
                        totalWeight = Mathf.Max(totalWeight, w);
                        break;
                }

            }

            if (goodInfluencers == 0)
            {
                totalWeight = 1f;
            }
            else
            {
                if (weightMode == WeightMode.Average)
                {
                    totalWeight /= goodInfluencers;
                }
            }

            totalWeight = Mathf.Lerp(1, totalWeight, influencerWeight);

            updateTile(tile, totalWeight * modifierWeight);
        }
    }

    virtual public void updateTile(Tile tile, float weight)
    {

    }
}
