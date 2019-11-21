#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace AssetIcons.Editors
{
	/// <summary>
	/// Handles the drawing, sizing and tinting of the assets.
	/// </summary>
	[InitializeOnLoad]
	public static class AssetIconDrawer
	{
		static AssetIconDrawer ()
		{
			
		}

		private const bool PreserveAspectRatio = true;
		private const bool SelectionTint = true;

		private const float LargeIconSize = 64.0f;

		/// <summary>
		/// Draws the custom icon for an asset using a sprite.
		/// </summary>
		/// <param name="guid">The GUID of the asset to draw.</param>
		/// <param name="rect">The Rect in which the item is drawn.</param>
		/// <param name="sprite">The sprite to draw for the asset icon.</param>
		public static void DrawCustomIcon (string guid, Rect rect, Sprite sprite)
		{
			Texture2D textureIcon;
			Rect textureRect;

			try
			{
				textureIcon = sprite.texture;
				textureRect = sprite.rect;
			}
			catch
			{
				AssetIconTools.RebuildIconDatabase ();
				return;
			}

			DrawCustomIcon (guid, rect, textureIcon, textureRect);
		}

		/// <summary>
		/// Draws the custom icon for an asset using a texture.
		/// </summary>
		/// <param name="guid">The GUID of the asset to draw.</param>
		/// <param name="rect">The Rect in which the item is drawn.</param>
		/// <param name="texture">The texture to draw for the asset icon.</param>
		public static void DrawCustomIcon (string guid, Rect rect, Texture texture)
		{
			Rect textureRect;

			try
			{
				textureRect = new Rect (0.0f, 0.0f, texture.width, texture.height);
			}
			catch
			{
				AssetIconTools.RebuildIconDatabase ();
				return;
			}

			DrawCustomIcon (guid, rect, texture, textureRect);
		}

		/// <summary>
		/// Draws the custom icon for an asset using a texture.
		/// </summary>
		/// <param name="guid">The GUID of the asset to draw.</param>
		/// <param name="rect">The Rect in which the item is drawn.</param>
		/// <param name="texture">The texture to draw for the asset icon.</param>
		/// <param name="textureRect">The rect of the texture to draw in pixels.</param>
		public static void DrawCustomIcon (string guid, Rect rect, Texture texture, Rect textureRect)
		{
			bool isSmall = IsIconSmall (ref rect);

			Rect normalisedTextureRect = new Rect (textureRect);

			normalisedTextureRect.x /= texture.width;
			normalisedTextureRect.width /= texture.width;

			normalisedTextureRect.y /= texture.height;
			normalisedTextureRect.height /= texture.height;

			DrawEmpty (rect);

			if (rect.width > LargeIconSize)
			{
				var offset = (rect.width - LargeIconSize) / 2.0f;
				rect = new Rect (rect.x + offset, rect.y + offset, LargeIconSize, LargeIconSize);
			}
			else
			{
#if UNITY_5_5
				if (isSmall)
					rect = new Rect (rect.x + 3, rect.y, rect.width, rect.height);
#elif UNITY_5_6_OR_NEWER
if (isSmall && !IsTreeView (rect))
rect = new Rect(rect.x + 3, rect.y, rect.width, rect.height);
#endif
			}

			bool selected = IsSelected (guid);

			/*
			if (selected && EditorWindow.focusedWindow.GetType () == ProjectBrowserType)
			{
				DrawSelectedEmpty (rect);
			}
			else
			{
				DrawEmpty (rect);
			}*/

			if (PreserveAspectRatio)
			{
				rect = FitWithAspectRatio (rect, textureRect);
			}

			Color originalBackgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = Color.white;

			if (SelectionTint && selected)
			{
				Color originalColor = GUI.color;
				GUI.color = Color.Lerp (GUI.skin.settings.selectionColor, Color.white, 0.35f);

				GUI.DrawTextureWithTexCoords (rect, texture, normalisedTextureRect, true);

				GUI.color = originalColor;
			}
			else
			{
				GUI.DrawTextureWithTexCoords (rect, texture, normalisedTextureRect, true);
			}

			GUI.backgroundColor = originalBackgroundColor;
		}

		/// <summary>
		/// Draws an empty square used to hide the pre-exisiting asset icon.
		/// </summary>
		/// <param name="rect">The rect in which to draw the square.</param>
		private static void DrawEmpty (Rect rect)
		{
			Color col = EditorGUIUtility.isProSkin
				? (Color) new Color32 (56, 56, 56, 255)
				: (Color) new Color32 (194, 194, 194, 255);

			EditorGUI.DrawRect (rect, col);
		}

		/// <summary>
		/// Draws an empty square used to hide the pre-exisiting asset icon.
		/// </summary>
		/// <param name="rect">The rect in which to draw the square.</param>
		private static void DrawSelectedEmpty (Rect rect)
		{
			Color col = new Color32 (62, 125, 231, 255);

			EditorGUI.DrawRect (rect, col);
		}

		/// <summary>
		/// Makes the originalRect fit into the other rect whilst preserving the aspect ratio of the textureRect.
		/// </summary>
		/// <returns>The rect fit inside the originalRect with the textureRect aspect ratio.</returns>
		/// <param name="bounds">The rect the result rect should fit in.</param>
		/// <param name="textureRect">Used to source the Aspect Ratio.</param>
		private static Rect FitWithAspectRatio (Rect bounds, Rect textureRect)
		{
			float ratio = textureRect.width / textureRect.height;

			Rect newRect = new Rect (bounds);

			float originalWidth = newRect.width;

			newRect.width = bounds.height * ratio;

			if (newRect.width > bounds.width)
			{
				float originalHeight = newRect.height;

				newRect.width = bounds.width;
				newRect.height = bounds.width / ratio;

				float heightDifference = originalHeight - newRect.height;
				newRect.y += heightDifference / 2;
			}
			else
			{
				float widthDifference = originalWidth - newRect.width;
				newRect.x += widthDifference / 2;
			}

			return newRect;
		}

		/// <summary>
		/// Determines if the specified guid is selected
		/// </summary>
		/// <returns><c>true</c> if is the specified GUID is selected; otherwise, <c>false</c>.</returns>
		/// <param name="guid">The GUID to check if selected.</param>
		private static bool IsSelected (string guid)
		{
			return Selection.assetGUIDs.Contains (guid);
		}


		/// <summary>
		/// Determines if the rect is being drawn in Tree View
		/// </summary>
		/// <returns><c>true</c> if is the specified rect is in TreeView; otherwise, <c>false</c>.</returns>
		/// <param name="rect">The rect used to check if it's being drawn in treeview.</param>
		private static bool IsTreeView (Rect rect)
		{
			return (rect.x - 16) % 14 == 0;
		}

		/// <summary>
		/// Determines if the rect should be drawn using a small icon.
		/// </summary>
		/// <returns><c>true</c> if the icon is small; otherwise, <c>false</c>.</returns>
		/// <param name="rect">The rect to check if it's small.</param>
		private static bool IsIconSmall (ref Rect rect)
		{
			var isSmall = rect.width > rect.height;

			if (isSmall)
				rect.width = rect.height;
			else
				rect.height = rect.width;

			return isSmall;
		}
	}
}
#endif