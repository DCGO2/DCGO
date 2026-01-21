using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AutoTextureConvert : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        TextureImporter textureImporter = assetImporter as TextureImporter;

        if(textureImporter.textureType == TextureImporterType.Cursor)
        {
            return;
        }

        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.maxTextureSize = 512;
    }
}