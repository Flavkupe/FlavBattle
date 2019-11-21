#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace AssetIcons.Editors
{
	/// <summary>
	/// Contains the information of the target used in the modules.
	/// </summary>
	public struct AssetTarget
	{
		public string FilePath;
		public string Extension;
		public string Filename;
		public string GUID;

		public static AssetTarget CreateFromGUID (string guid)
		{
			AssetTarget newAssetTarget = new AssetTarget ();

			newAssetTarget.GUID = guid;
			newAssetTarget.FilePath = AssetDatabase.GUIDToAssetPath (guid);
			newAssetTarget.Extension = Path.GetExtension (newAssetTarget.FilePath);
			newAssetTarget.Filename = Path.GetFileName (newAssetTarget.FilePath);

			return newAssetTarget;
		}

		public static AssetTarget CreateFromPath (string path)
		{
			return CreateFromGUID (AssetDatabase.AssetPathToGUID (path));
		}
	}
}
#endif