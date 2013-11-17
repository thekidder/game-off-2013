using UnityEngine;
using UnityEditor;

public class TexturePreProcessor : AssetPostprocessor
{
    void OnPreprocessTexture ()
    {
        TextureImporter importer = assetImporter as TextureImporter;
        Object asset = AssetDatabase.LoadAssetAtPath (importer.assetPath, typeof(Texture2D));
        if (!asset) {
            importer.textureType = TextureImporterType.Sprite;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false;
            importer.spritePixelsToUnits = 8;
            importer.filterMode = FilterMode.Point;
        }    
    }
}
