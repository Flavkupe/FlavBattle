#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace AssetIcons.Editors
{
	/// <summary>
	/// Used to specify that this member controls the ScriptableObject's Assets' Icon.
	/// </summary>
	[CreateAssetMenu]
	public class ExtensionTypeModule : AssetIconModule
	{
		/// <summary>
		/// These are extensions that Unity draw icons for which should be drawn using a different method.
		/// </summary>
		private static readonly string[] InvalidExtensions = new string[] { 
			"png", "psd", "tiff", "jpg", "tga", "gif", "bmp", "iff", "pict", "asset" };

		/// <summary>
		/// The extension the 
		/// </summary>
		[SerializeField]
		private string ExtensionName = "";

		/// <summary>
		/// Used to mark the lastExtensionValid variable as dirty.
		/// </summary>
		[NonSerialized]
		private bool isExtensionDirty = true;

		/// <summary>
		/// Cached result of the validitiy checks.
		/// </summary>
		[NonSerialized]
		private bool lastExtensionValid = false;

		//public string Style = "Width = 100%; Height = 100%;


		/// <summary>
		/// Checks to see if the module should draw on the passed target.
		/// </summary>
		/// <returns><c>true</c>, if target was evaluated sucessfully, <c>false</c> otherwise.</returns>
		/// <param name="filename">The filename of the target.</param>
		/// <param name="extension">The extension of the target.</param>
		public override bool EvaluateTarget (AssetTarget assetTarget)
		{
			if (!Active)
				return false;

			if (!IsValid ())
				return false;
			
			if (assetTarget.Extension == "." + ExtensionName)
				return true;
			
			return false;
		}

		/// <summary>
		/// Marks the extensions type as dirty, forcing the validity of the extension to be rechecked.
		/// </summary>
		public void SetDirtyExtension ()
		{
			isExtensionDirty = true;
		}

		/// <summary>
		/// Checks if the extension of the module is valid.
		/// </summary>
		/// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
		public bool IsValid ()
		{
			if (isExtensionDirty)
			{
				// We don't check if it's already in use, because we don't really care - it's just a warning.
				lastExtensionValid = IsValidExtension (ExtensionName);
			}

			return lastExtensionValid;
		}

		/// <summary>
		/// Checks AssetIconTools.ExtensionDrawers for other ExtensionTypeModules and compares extension names.
		/// </summary>
		/// <returns><c>true</c> if this instance shares extension names with another; otherwise, <c>false</c>.</returns>
		public bool IsInUseExtension ()
		{
			bool hasFoundSelf = false;

			List<ExtensionTypeModule> matches = new List<ExtensionTypeModule> ();

			for (int i = 0; i < ModuleManifest.ExtensionDrawers.Count; i++)
			{
				if (!Active)
					return false;

				ExtensionTypeModule module = (ExtensionTypeModule)ModuleManifest.ExtensionDrawers [i];

				if (module == this)
				{
					hasFoundSelf = true;
					continue;
				}

				if (module != null)
				{
					if (!module.Active)
						continue;
					
					if (module.ExtensionName == ExtensionName)
						matches.Add (module);
				}
			}

			if (!hasFoundSelf)
			{
				ModuleManifest.ExtensionDrawers.Add (this);
			}

			if (matches.Count != 0)
				return true;

			return false;
		}

		/// <summary>
		/// Checks if the passed extension is contained in InvalidExtensions.
		/// </summary>
		/// <returns><c>true</c> if the extension is valid; otherwise, <c>false</c>.</returns>
		/// <param name="extension">Extension.</param>
		public static bool IsValidExtension (string extension)
		{
			for (int i = 0; i < InvalidExtensions.Length; i++)
			{
				if (InvalidExtensions [i] == extension)
					return false;
			}

			return true;
		}
	}

	/// <summary>
	/// Custom Editor for ExtensionTypeModule which provides a clean interface for setting up the module.
	/// </summary>
	[CustomEditor (typeof (ExtensionTypeModule), true)]
	class ExtensionTypeModuleEditor : BaseModuleEditor
	{
		ExtensionTypeModule targetExtensionModule;
		SerializedProperty extensionNameProp;

		AnimBool AnimError_ExtensionEmpty;

		bool isExtensionEmpty = false;
		bool isExtensionValid = false;
		bool isExtensionInUse = false;

		public override void OnEnable ()
		{
			base.OnEnable ();
			extensionNameProp = serializedObject.FindProperty ("ExtensionName");

			AnimError_ExtensionEmpty = new AnimBool (false);
			AnimError_ExtensionEmpty.valueChanged.AddListener (Repaint);

			targetExtensionModule = (ExtensionTypeModule)target;

			UpdateErrors ();
		}

		public override void OnInspectorGUI ()
		{
			EditorGUI.BeginChangeCheck ();

			base.OnInspectorGUI ();

			DrawHeader ("Extension");
			EditorGUILayout.BeginVertical (EditorStyles.helpBox);

			DrawDescription ("The file extension (.txt) the icon is drawn on.");


			BeginChangedCheck ();
			EditorGUILayout.PropertyField (extensionNameProp);
			EndChangedCheck ();

			if (EditorGUI.EndChangeCheck ())
			{
				serializedObject.ApplyModifiedProperties ();

				string extensionName = extensionNameProp.stringValue;

				extensionName = extensionName.ToLower ();
				extensionName = extensionName.Replace (".", "");
				extensionName = extensionName.Replace (" ", "");

				extensionNameProp.stringValue = extensionName;
				serializedObject.ApplyModifiedPropertiesWithoutUndo ();

				targetExtensionModule.SetDirtyExtension ();
				UpdateErrors ();
			}

			if (EditorGUILayout.BeginFadeGroup (AnimError_ExtensionEmpty.faded))
			{
				if (isExtensionEmpty)
					EditorGUILayout.HelpBox ("This extension is empty.", MessageType.Warning);

				if (!isExtensionValid)
					EditorGUILayout.HelpBox ("This extension is not valid.", MessageType.Warning);

				if (isExtensionInUse)
					EditorGUILayout.HelpBox ("This extension is already in use.", MessageType.Warning);

				if (AnimError_ExtensionEmpty.target == false)
					EditorGUILayout.HelpBox ("", MessageType.Warning);
			}
			else
			{
				GUILayout.Space ((1.0f - AnimError_ExtensionEmpty.faded) * 5.0f);
			}
			EditorGUILayout.EndFadeGroup ();



			EditorGUILayout.EndVertical ();
			serializedObject.ApplyModifiedProperties ();
		}

		void UpdateErrors ()
		{
			isExtensionEmpty = extensionNameProp.stringValue == "";
			isExtensionValid = ExtensionTypeModule.IsValidExtension (extensionNameProp.stringValue);
			isExtensionInUse = targetExtensionModule.IsInUseExtension ();

			AnimError_ExtensionEmpty.target = isExtensionEmpty || !isExtensionValid || isExtensionInUse;
		}
	}
}
#endif