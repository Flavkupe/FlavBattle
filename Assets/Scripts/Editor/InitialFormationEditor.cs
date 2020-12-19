using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FormationData))]
public class InitialFormationEditor : Editor
{
    private GameObject _go;
    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnInspectorGUI()
    {
        // serializedObject.Update();

        DrawDefaultInspector();



        // serializedObject.ApplyModifiedProperties();
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        base.OnPreviewGUI(r, background);

        var data = target as FormationData;
        if (data != null && data.InitialFormation != null)
        {
            // var sprite = data.InitialFormation?.GetOfficer()?.Sprite;
            var f = data.InitialFormation;
            var c = r.center.Shift(-16, -16);
            var space = 64;
            var half = space / 2;
            DrawTextureGUI(new Rect(c.x, c.y - space, 32, 32), f.BL?.Sprite);
            DrawTextureGUI(new Rect(c.x - half, c.y - half, 32, 32), f.BM?.Sprite);
            DrawTextureGUI(new Rect(c.x - space, c.y, 32, 32), f.BR?.Sprite);
            DrawTextureGUI(new Rect(c.x + half, c.y - half, 32, 32), f.ML?.Sprite);
            DrawTextureGUI(new Rect(c.x, c.y, 32, 32), f.MM?.Sprite);
            DrawTextureGUI(new Rect(c.x - half, c.y + half, 32, 32), f.MR?.Sprite);
            DrawTextureGUI(new Rect(c.x + space, c.y, 32, 32), f.FL?.Sprite);
            DrawTextureGUI(new Rect(c.x + half, c.y + half, 32, 32), f.FM?.Sprite);
            DrawTextureGUI(new Rect(c.x, c.y + space, 32, 32), f.FR?.Sprite);
        }
    }

    public static void DrawTextureGUI(Rect position, Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                                   sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
        Vector2 actualSize = new Vector2(position.width, position.height);

        actualSize.y *= (sprite.rect.height / sprite.rect.width);
        GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (position.height - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);
    }
}

