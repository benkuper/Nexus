using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TileModifier : MonoBehaviour
{
    List<TileModifierInfluencer> influencers = new List<TileModifierInfluencer>();

    Transform tileContainer;

    void OnEnable()
    {
        tileContainer = transform.parent.Find("TilesContainer");
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        updateTiles();
    }

    virtual public void updateTiles()
    {
        if(tileContainer == null)
        {
            return;
        }

        Transform[] tiles = tileContainer.GetComponentsInChildren<Transform>();
        foreach (Transform tile in tiles)
        {
            float totalWeight = 0f;
            foreach (TileModifierInfluencer influencer in influencers)
            {
                totalWeight += influencer.getWeightAtPos(tile.position);
            }
            updateTile(tile, totalWeight);
        }
    }

    virtual public void updateTile(Transform tile, float weight)
    {
        
    }
}
