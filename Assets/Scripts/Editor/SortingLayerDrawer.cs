using FlavBattle.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(SortingLayerValues))]
public class SortingLayerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var layers = SortingLayer.layers;
        var names = layers.Select(l => l.name).ToList();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Sorting Layer");
        var layerNameProp = property.FindPropertyRelative(nameof(SortingLayerValues.Name));
        var layerValueProp = property.FindPropertyRelative(nameof(SortingLayerValues.Value));

        var index = names.IndexOf(layerNameProp.stringValue);
        if (index == -1)
        {
            index = 0;
        }

        index = EditorGUILayout.Popup(index, names.ToArray());
        layerValueProp.intValue = EditorGUILayout.IntField(layerValueProp.intValue);

        layerNameProp.stringValue = names[index];

        EditorGUILayout.EndHorizontal();
    }
}

