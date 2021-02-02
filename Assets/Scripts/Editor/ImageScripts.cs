using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class ImageScripts
{
    [MenuItem("Assets/Utils/Normalize")]
    public static void Normalize()
    {
        var selected = Selection.activeObject as Texture2D;
        if (selected != null)
        {
            var path = "Assets";
            var other = AssetDatabase.GetAssetPath(selected);
            if (other != null)
            {
                path = other;
            }

            path = Path.GetDirectoryName(path);
            path = Path.Combine(path, "NormalMap.png");
                
            NormalMap(selected, 1.0f, path);
        }
        else
        {
            Debug.LogWarning("Must select a Texture2D object!");
        }
    }

    private static Texture2D NormalMap(Texture2D source, float strength, string path)
    {
        strength = Mathf.Clamp(strength, 0.0F, 1.0F);

        Texture2D normalTexture;
        float xLeft;
        float xRight;
        float yUp;
        float yDown;
        float yDelta;
        float xDelta;

        normalTexture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true);

        for (int y = 0; y < normalTexture.height; y++)
        {
            for (int x = 0; x < normalTexture.width; x++)
            {
                xLeft = source.GetPixel(x - 1, y).grayscale * strength;
                xRight = source.GetPixel(x + 1, y).grayscale * strength;
                yUp = source.GetPixel(x, y - 1).grayscale * strength;
                yDown = source.GetPixel(x, y + 1).grayscale * strength;
                xDelta = ((xLeft - xRight) + 1) * 0.5f;
                yDelta = ((yUp - yDown) + 1) * 0.5f;
                normalTexture.SetPixel(x, y, new Color(xDelta, yDelta, 1.0f, yDelta));
            }
        }
        normalTexture.Apply();

        //Code for exporting the image to assets folder
        Debug.Log($"Writing to {path}");
        System.IO.File.WriteAllBytes(path, normalTexture.EncodeToPNG());

        return normalTexture;
    }
}

