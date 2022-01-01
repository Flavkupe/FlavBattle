using FlavBattle.Resources;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ResourceUtils
{
    public static void WriteToFile(StringResourceMap resourceMap, StringResourceCategory category, string key, string content)
    {
        var textAsset = GetTextAsset();
        if (textAsset == null)
        {
            return;
        }

        resourceMap.Set(category, key, content);
        var json = resourceMap.ToJSON();

        StringResources.UpdateStrings(resourceMap);

        File.WriteAllText(AssetDatabase.GetAssetPath(textAsset), json);
        EditorUtility.SetDirty(textAsset);
    }

    private static TextAsset GetTextAsset()
    {
        var textAsset = StringResources.GetTextAsset();
        if (textAsset == null)
        {
            Debug.LogWarning("Error getting text asset");
            return null;
        }

        return textAsset;
    }
}
