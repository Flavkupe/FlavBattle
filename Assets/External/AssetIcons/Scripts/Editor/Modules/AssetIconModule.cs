#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace AssetIcons.Editors
{
	/// <summary>
	/// The base module, used to evaluate targets and draw simple static icons. 
	/// </summary>
	public abstract class AssetIconModule : ScriptableObject
	{
		public bool Active = true;
		public Sprite Icon;

		public abstract bool EvaluateTarget (AssetTarget assetTarget);

		public virtual void Draw (string guid, Rect rect)
		{
			AssetIconDrawer.DrawCustomIcon (guid, rect, Icon);
		}

		/// <summary>
		/// Gets the icon to draw.
		/// </summary>
		/// <returns>The icon to draw.</returns>
		[AssetIcon]
		public virtual Sprite GetIcon ()
		{
			return Icon;
		}
	}


	[CustomEditor (typeof (AssetIconModule), true)]
	class BaseModuleEditor : Editor
	{
		SerializedProperty activeProp;
		SerializedProperty iconProp;

		GUIStyle headerStyle = null;

		GUIStyle HeaderStyle
		{
			get
			{
				if (headerStyle == null)
				{
					headerStyle = new GUIStyle (EditorStyles.label);
					headerStyle.alignment = TextAnchor.MiddleCenter;
					headerStyle.fontSize = 12;
					headerStyle.padding.top = 12;
					headerStyle.normal.textColor = new Color (0.2f, 0.2f, 0.2f);
				}

				return headerStyle;
			}
		}

		public virtual void OnEnable ()
		{
			activeProp = serializedObject.FindProperty ("Active");
			iconProp = serializedObject.FindProperty ("Icon");
		}

		public override void OnInspectorGUI ()
		{
			DrawHeader ("General");
			EditorGUILayout.BeginVertical (EditorStyles.helpBox);

			BeginChangedCheck ();

			EditorGUILayout.PropertyField (activeProp);
			EditorGUILayout.PropertyField (iconProp);

			EndChangedCheck ();

			EditorGUILayout.EndVertical ();
			serializedObject.ApplyModifiedProperties ();
		}

		protected void DrawHeader (string content)
		{
			EditorGUILayout.LabelField (content, HeaderStyle, GUILayout.Height (32));
		}

		protected void DrawDescription (string content)
		{
			EditorGUILayout.LabelField (content, EditorStyles.centeredGreyMiniLabel);
		}

		protected void BeginChangedCheck ()
		{
			EditorGUI.BeginChangeCheck ();
		}

		protected void EndChangedCheck ()
		{
			if (EditorGUI.EndChangeCheck ())
			{
				EditorApplication.RepaintProjectWindow ();
			}
		}
	}
}
#endif