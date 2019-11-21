#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace AssetIcons.Editors
{
	/// <summary>
	/// Keeps a record of all modules currently loaded by AssetIcons.
	/// </summary>
	public class ModuleManifest : UnityEditor.AssetModificationProcessor
	{
		public static List<AssetIconModule> ExtensionDrawers;

		static ModuleManifest ()
		{
			if (!ReloadModules ())
			{
				EditorApplication.delayCall += () => {
					ReloadModules ();
				};
			}
		}

		/// <summary>
		/// Reloads all modules from the resources.
		/// </summary>
		/// <returns><c>true</c>, if modules where successfully reloaded, <c>false</c> otherwise.</returns>
		public static bool ReloadModules ()
		{
			AssetIconModule[] ExtensionDrawersArray = Resources.LoadAll <AssetIconModule> ("");

			if (ExtensionDrawersArray == null)
				return false;

			if (ExtensionDrawersArray.Length == 0)
				return false;

			ModuleManifest.ExtensionDrawers = new List<AssetIconModule> (ExtensionDrawersArray);

			return true;
		}


		/// <summary>
		/// Listens for the creation of assets and adds them to the ExtensionDrawers 
		/// </summary>
		/// <param name="path">The path of the created asset.</param>
		static void OnWillCreateAsset (string path)
		{
			string pathExtension = Path.GetExtension (path);

			if (pathExtension != ".asset")
				return;

			AssetIconModule loadedIcon = AssetDatabase.LoadAssetAtPath <AssetIconModule> (path);

			if (loadedIcon != null)
			{
				if (!ExtensionDrawers.Contains (loadedIcon))
				{
					ExtensionDrawers.Add (loadedIcon);
				}
			}
		}

		/// <summary>
		/// Listens for the saving of assets and adds them to the ExtensionDrawers, primarily used to add Duplicated modules
		/// to the manifest.
		/// </summary>
		/// <param name="paths">The path of the saved assets.</param>
		static string[] OnWillSaveAssets (string[] paths)
		{
			foreach (string path in paths)
			{
				string pathExtension = Path.GetExtension (path);

				if (pathExtension != ".asset")
					return paths;
				
				AssetIconModule loadedIcon = AssetDatabase.LoadAssetAtPath <AssetIconModule> (path);

				if (loadedIcon != null)
				{
					if (!ExtensionDrawers.Contains (loadedIcon))
					{
						ExtensionDrawers.Add (loadedIcon);
					}
				}
			}

			return paths;
		}
	}
}
#endif