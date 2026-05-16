using System.Collections.Generic;
using UnityEditor.Rendering.HighDefinition;
using UnityEngine;
using UnityEngine.UIElements;

public class TileMatModifier : TileModifier
{
    public Gradient colorOverWeight;
    public Gradient borderOverWeight;
    public float borderWidth = .1f;
    public float borderIntensity = 1f;
    public float textureWeight = 1f;
    public AnimationCurve weightBorderCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    MaterialPropertyBlock materialBlock;

    public override void updateTile(Tile tile, float weight)
    {
        base.updateTile(tile, weight);

        Renderer renderer = tile.GetComponentInChildren<Renderer>();
        Material sharedMaterial = renderer.sharedMaterial;

        if (sharedMaterial == null)
        {
            return;
        }
        if (materialBlock == null)
        {
            materialBlock = new MaterialPropertyBlock();
        }
        renderer.GetPropertyBlock(materialBlock);


        if (sharedMaterial.HasProperty(TileController.BaseColorPropertyId))
        {
            Color baseColor = materialBlock.GetColor(TileController.BaseColorPropertyId);
            Color gradientColor = colorOverWeight.Evaluate(weight);
            Color finalColor = Color.Lerp(baseColor, gradientColor, weight);
            materialBlock.SetColor(TileController.BaseColorPropertyId, finalColor);
        }

        if (sharedMaterial.HasProperty(TileController.BorderColorPropertyId))
        {
            Color borderColor = materialBlock.GetColor(TileController.BorderColorPropertyId);
            Color gradientBorderColor = borderOverWeight.Evaluate(weight);
            Color finalBorderColor = Color.Lerp(borderColor, gradientBorderColor, weight);
            materialBlock.SetColor(TileController.BorderColorPropertyId, finalBorderColor);
        }

        if (sharedMaterial.HasProperty(TileController.BorderWidthPropertyId))
        {
            float initWidth = materialBlock.GetFloat(TileController.BorderWidthPropertyId);
            float finalBorderWidth = Mathf.Lerp(initWidth, borderWidth, weight);
            materialBlock.SetFloat(TileController.BorderWidthPropertyId, finalBorderWidth);
        }

        if (sharedMaterial.HasProperty(TileController.BorderIntensityPropertyId))
        {
            float initIntensity = materialBlock.GetFloat(TileController.BorderIntensityPropertyId);
            float finalBorderIntensity = Mathf.Lerp(initIntensity, borderIntensity, weightBorderCurve.Evaluate(weight));
            materialBlock.SetFloat(TileController.BorderIntensityPropertyId, finalBorderIntensity);
        }

        if(sharedMaterial.HasProperty(TileController.TextureWeightPropertyId))
        {
            float initTextureWeight = materialBlock.GetFloat(TileController.TextureWeightPropertyId);
            float finalTextureWeight = Mathf.Lerp(initTextureWeight, textureWeight, weight);
            materialBlock.SetFloat(TileController.TextureWeightPropertyId, finalTextureWeight);
        }



        renderer.SetPropertyBlock(materialBlock);
    }
}