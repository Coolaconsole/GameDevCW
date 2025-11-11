using UnityEditor;
using UnityEngine;

public static class PrefabThumbnailGenerator
{
    [MenuItem("Tools/Generate Prefab Thumbnail")]
    public static void GenerateThumbnail()
    {
        GameObject prefab = Selection.activeGameObject;
        if (prefab == null) return;

        Texture2D preview = AssetPreview.GetAssetPreview(prefab);
        if (preview == null) preview = AssetPreview.GetMiniThumbnail(prefab);

        byte[] png = preview.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/Images/UI" + prefab.name + ".png", png);
        Debug.Log("Thumbnail saved for " + prefab.name);
    }
}