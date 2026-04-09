using System.Collections.Generic;
using UnityEngine;

public class TileMatMasker : TileModifier
{
    public List<int> invisibeRows = new List<int>();
    public List<int> invisibeCols = new List<int>();

    public override void updateTile(Tile tile, float weight)
    {
        if (invisibeRows.Contains(tile.y) || invisibeCols.Contains(tile.x))
        {
            tile.GetComponentInChildren<Renderer>().enabled = false;
        }
        else
        {
            tile.GetComponentInChildren<Renderer>().enabled = true;
        }


    }
}