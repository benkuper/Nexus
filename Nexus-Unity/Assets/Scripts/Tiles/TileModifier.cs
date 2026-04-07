using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TileModifier : MonoBehaviour
{
    public List<TileModifierInfluencer> influencers = new List<TileModifierInfluencer>();

    Transform tileContainer;

    [Range(0f, 1f)]
    public float modifierWeight = 1f;
    void OnEnable()
    {
        tileContainer = transform.Find("TilesContainer");
    }

    // Update is called once per frame
    virtual protected void Update()
    {
    }

    virtual public void updateTiles()
    {
        if (tileContainer == null)
        {
            tileContainer = transform.Find("TilesContainer");
        }

        if (tileContainer == null)
        {
            return;
        }

        Tile[] tiles = tileContainer.GetComponentsInChildren<Tile>();
        foreach (Tile tile in tiles)
        {
            int goodInfluencers = 0;
            float totalWeight = 0f;
            foreach (TileModifierInfluencer influencer in influencers)
            {
                if (influencer == null || !influencer.enabled)
                {
                    continue;
                }
                goodInfluencers++;
                totalWeight += influencer.getWeightAtPos(tile.transform.position);
            }

            if (goodInfluencers == 0)
            {
                totalWeight = 1f;
            }
            else
            {
                totalWeight /= goodInfluencers;
            }

            updateTile(tile, totalWeight * modifierWeight);
        }
    }

    virtual public void updateTile(Tile tile, float weight)
    {

    }
}
