#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetIcons.Editors
{
	/// <summary>
	/// Draws the AssetIcon property, which repaints the project window when the icon is changed.
	/// </summary>
	[CustomPropertyDrawer (typeof (AssetIconAttribute))]
	class AssetIconAttributeDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginChangeCheck ();

			EditorGUI.PropertyField (position, property);

			if (EditorGUI.EndChangeCheck ())
			{
				EditorApplication.RepaintProjectWindow ();
			}
		}
	}
}
#endif