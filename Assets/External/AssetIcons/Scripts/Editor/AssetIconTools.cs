#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace AssetIcons.Editors
{
	/// <summary>
	/// Manages all AssetIcon Attributes across the project and uses <see cref="AssetIcons.AssetIconDrawer"/>
	/// to draw them.
	/// </summary>
	[InitializeOnLoad]
	static class AssetIconTools
	{
		private const string MenuLocation = "Assets/AssetIcons";
		private const int MenuPriority = 2100000000;


		/// <summary>
		/// A basic interface for supplying an object return type in a given context.
		/// </summary>
		private interface IIconProvider
		{
			object GetIcon (object context);
		}

		/// <summary>
		/// Returns an object from a FieldInfo in a given context.
		/// </summary>
		private class FieldIconProvider : IIconProvider
		{
			FieldInfo field;

			public FieldIconProvider (FieldInfo _field)
			{
				field = _field;
			}

			public object GetIcon (object context)
			{
				return field.GetValue (context);
			}
		}

		/// <summary>
		/// Returns an object from a MethodInfo in a given context.
		/// </summary>
		private class MethodIconProvider : IIconProvider
		{
			MethodInfo method;

			public MethodIconProvider (MethodInfo _method)
			{
				method = _method;
			}

			public object GetIcon (object context)
			{
				return method.Invoke (context, null);
			}
		}

		public delegate void IconDelegate (Rect area, string GUID, bool isSmall);

		/// <summary>
		/// The binding flags used to search for the AssetIconAttribute
		/// </summary>
		private const BindingFlags AttributeBindingflags = BindingFlags.Instance | BindingFlags.Static |
			BindingFlags.Public | BindingFlags.NonPublic;

		private static readonly string[] newLineSeperators = new string[] {"\r\n", "\n", "\r"};
		private static Dictionary<Type, IIconProvider> IconProviders;
		private static bool isEnabled = true;


		/// <summary>
		/// Initializes the <see cref="AssetIcons.AssetIconTools"/> class.
		/// </summary>
		static AssetIconTools ()
		{
			IconProviders = BuildIconDatabase ();

#if UNITY_2017_1_OR_NEWER
			try
			{
				Assembly editorAssembly = Assembly.GetAssembly (typeof(SerializedProperty));
				Type ObjectListAreaType = editorAssembly.GetType ("UnityEditor.ObjectListArea");

				EventInfo postAssetIconDrawCallbackEvent = ObjectListAreaType.GetEvent ("postAssetIconDrawCallback", BindingFlags.Static | BindingFlags.NonPublic);

				IconDelegate eventHandler = AllItemsGUI;

				MethodInfo addMethod = postAssetIconDrawCallbackEvent.GetAddMethod (true);

				Type delegateType = ObjectListAreaType.GetNestedType ("OnAssetIconDrawDelegate", BindingFlags.NonPublic);

				object param1 = Cast (eventHandler, delegateType);

				addMethod.Invoke (null, new object[1] { param1 });
			}
			catch
			{
				EditorApplication.projectWindowItemOnGUI += ItemOnGUI;
			}
#else
			EditorApplication.projectWindowItemOnGUI += ItemOnGUI;
#endif

			EditorApplication.delayCall += () => {
				isEnabled = EditorPrefs.GetBool ("AssetIconUtility_Draw", true);

				Menu.SetChecked (MenuLocation + "/Draw Custom Asset Icons", isEnabled);
			};
		}

		/// <summary>
		///	Creates a new extension type drawer asset at the designated path.
		/// </summary>
		[MenuItem (MenuLocation + "/Create Extension Type Module", false, MenuPriority)]
		public static void CreateExtensionTypeModule ()
		{
			CreateGenericModule <ExtensionTypeModule> ("Extension Type Module");
		}

		/// <summary>
		/// Toggles the editor drawing of the asset icons. Used to disable AssetIcon
		/// </summary>
		[MenuItem (MenuLocation + "/Draw Custom Asset Icons", false, MenuPriority + 11)]
		public static void ToggleEditorDrawing ()
		{
			isEnabled = !isEnabled;

			EditorPrefs.SetBool ("AssetIconUtility_Draw", isEnabled);
			Menu.SetChecked (MenuLocation + "/Draw Custom Asset Icons", isEnabled);

			EditorApplication.RepaintProjectWindow ();
		}

		public static void RebuildIconDatabase ()
		{
			IconProviders = BuildIconDatabase ();
		}

#if UNITY_2017_1_OR_NEWER
		private static void AllItemsGUI (Rect rect, string guid, bool isSmall)
		{
			ItemOnGUI (guid, rect);
		}
#endif

		/// <summary>
		/// Paints the item in the project window. 
		/// </summary>
		/// <param name="guid">The GUID of the asset to check.</param>
		/// <param name="rect">The Rect in which the item is drawn.</param>
		private static void ItemOnGUI (string guid, Rect rect)
		{
			if (Event.current.type != EventType.Repaint || string.IsNullOrEmpty(guid))
				return;

			if (!isEnabled)
				return;

			AssetTarget assetTarget = AssetTarget.CreateFromGUID (guid);

			if (assetTarget.Extension == "")
				return;

			if (assetTarget.Extension == ".asset")
			{
				UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath (assetTarget.FilePath, typeof (UnityEngine.Object)) as UnityEngine.Object;

				if (obj == null)
					return;

				Type type = obj.GetType ();

				IIconProvider iconProvider;
				IconProviders.TryGetValue (type, out iconProvider);

				if (iconProvider != null)
				{
					object objectIcon = iconProvider.GetIcon (obj);

					if (objectIcon == null)
						return;

					Type iconType = objectIcon.GetType ();

					if (typeof (Sprite).IsAssignableFrom (iconType))
					{
						Sprite spriteIcon = (Sprite)objectIcon;

						if (spriteIcon != null) {
							AssetIconDrawer.DrawCustomIcon (guid, rect, spriteIcon);
							return;
						}
					}
					else if (typeof (Texture).IsAssignableFrom (iconType))
					{
						Texture textureIcon = (Texture)objectIcon;

						if (textureIcon != null) {
							AssetIconDrawer.DrawCustomIcon (guid, rect, textureIcon);
							return;
						}
					}
				}
			}

			if (ModuleManifest.ExtensionDrawers != null)
			{
				for (int i = 0; i < ModuleManifest.ExtensionDrawers.Count; i++)
				{
					AssetIconModule module = ModuleManifest.ExtensionDrawers [i];

					if (module == null)
					{
						ModuleManifest.ExtensionDrawers.Remove (null);
						continue;
					}

					if (module.EvaluateTarget (assetTarget))
					{
						module.Draw (guid, rect);
						return;
					}
				}
			}
		}

		/// <summary>
		/// Creates a dictionary of all of the types that use the AssetIconAttribute and their IconProviders
		/// </summary>
		static Dictionary<Type, IIconProvider> BuildIconDatabase ()
		{
			Dictionary<Type, IIconProvider> iconProviders = new Dictionary<Type, IIconProvider> ();

			Assembly[] CheckAssemblies = AppDomain.CurrentDomain.GetAssemblies ();

			for (int a = 0; a < CheckAssemblies.Length; a++)
			{
				Assembly assembly = CheckAssemblies[a];

				Type[] assemblyTypes = assembly.GetTypes ();
				for (int i = 0; i < assemblyTypes.Length; i++)
				{
					Type type = assemblyTypes [i];

					if (!typeof(ScriptableObject).IsAssignableFrom (type))
						continue;

					bool hasFoundAttribute = false;

					FieldInfo[] typeFields = type.GetFields (AttributeBindingflags);
					PropertyInfo[] typeProperties = type.GetProperties (AttributeBindingflags);
					MethodInfo[] typeMethods = type.GetMethods (AttributeBindingflags);


					for (int j = 0; j < typeFields.Length; j++)
					{
						FieldInfo field = typeFields [j];

						object[] attributeObjects = field.GetCustomAttributes (typeof(AssetIconAttribute), true);

						if (attributeObjects.Length == 1)
						{
							if (hasFoundAttribute)
							{
								MultipleAttributesError (type);
								break;
							}

							if (!IsSupportedType (field.FieldType))
							{
								UnsupportedTypeError (type);
								break;
							}

							iconProviders.Add (type, new FieldIconProvider (field));
							hasFoundAttribute = true;
						}
					}

					for (int j = 0; j < typeProperties.Length; j++)
					{
						PropertyInfo property = typeProperties [j];

						object[] attributeObjects = property.GetCustomAttributes (typeof(AssetIconAttribute), true);

						if (attributeObjects.Length == 1)
						{
							if (hasFoundAttribute)
							{
								MultipleAttributesError (type);
								break;
							}

							if (!IsSupportedType (property.PropertyType))
							{
								UnsupportedTypeError (type);
								break;
							}

							iconProviders.Add (type, new MethodIconProvider (property.GetGetMethod ()));
							hasFoundAttribute = true;
						}
					}

					for (int j = 0; j < typeMethods.Length; j++)
					{
						MethodInfo method = typeMethods [j];

						object[] attributeObjects = method.GetCustomAttributes (typeof(AssetIconAttribute), true);

						if (attributeObjects.Length == 1)
						{
							if (hasFoundAttribute)
							{
								MultipleAttributesError (type);
								break;
							}

							if (!IsSupportedType (method.ReturnType)) {
								UnsupportedTypeError (type);
								break;
							}

							if (IsSupportedParameters (method.GetParameters ()))
							{
								UnsupportedParametersError (type);
								break;
							}

							iconProviders.Add (type, new MethodIconProvider (method));
							hasFoundAttribute = true;
						}
					}
				}
			}

			return iconProviders;
		}

		/// <summary>
		///	Creates a generic asset used for the creation of different modules.
		/// </summary>
		private static void CreateGenericModule <T> (string name)
			where T : AssetIconModule
		{
			T asset = ScriptableObject.CreateInstance<T> ();

			MonoScript script = MonoScript.FromScriptableObject (asset);

			string scriptPath = AssetDatabase.GetAssetPath (script);
			int lastIndex = scriptPath.LastIndexOf ('/');

			string parentFolder = scriptPath.Substring (0, lastIndex);
			string path = parentFolder + "/Resources";

			AssetDatabase.Refresh ();
			if (!AssetDatabase.IsValidFolder (path))
			{
				AssetDatabase.CreateFolder (parentFolder, "Resources");
				AssetDatabase.Refresh ();
			}

			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/" + name + ".asset");
			AssetDatabase.CreateAsset (asset, assetPathAndName);

			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();
			EditorUtility.FocusProjectWindow ();
			Selection.activeObject = asset;
		}

		/// <summary>
		/// Converts a delegate type to the supplied type. Used in reflection
		/// </summary>
		/// <param name="source">Source delegate.</param>
		/// <param name="type">New Type forr the delegate.</param>
		public static Delegate Cast (Delegate source, Type type)
		{
			if (source == null)
				return null;

			Delegate[] delegates = source.GetInvocationList ();

			if (delegates.Length == 1)
			{
				return Delegate.CreateDelegate (type, delegates[0].Target, delegates[0].Method);
			}

			Delegate[] delegatesDest = new Delegate[delegates.Length];

			for (int nDelegate = 0; nDelegate < delegates.Length; nDelegate++)
			{
				delegatesDest[nDelegate] = Delegate.CreateDelegate (type, delegates[nDelegate].Target,
					delegates[nDelegate].Method);
			}

			return Delegate.Combine (delegatesDest);
		}


		/// <summary>
		/// Determines if the parameters passed are supported in AssetIcons.
		/// </summary>
		/// <returns><c>true</c> if the parameters passed are supported; otherwise, <c>false</c>.</returns>
		/// <param name="parameterTypes">The parameters to compare.</param>
		private static bool IsSupportedParameters (ParameterInfo[] parameterInfos)
		{
			Type[] parameterTypes = new Type[parameterInfos.Length];

			for (int k = 0; k < parameterInfos.Length; k++)
			{
				parameterTypes[k] = parameterInfos [k].ParameterType;
			}

			return IsSupportedParameters (parameterTypes);
		}

		/// <summary>
		/// Determines if the parameters passed are supported in AssetIcons.
		/// </summary>
		/// <returns><c>true</c> if the parameters passed are supported; otherwise, <c>false</c>.</returns>
		/// <param name="parameterTypes">The parameters to compare.</param>
		private static bool IsSupportedParameters (Type[] parameterTypes)
		{
			return parameterTypes.Length != 0;
		}

		/// <summary>
		/// Determines if the type is supported as an icon in AssetIcons
		/// </summary>
		/// <param name="type">The type to compare.</param>
		private static bool IsSupportedType (Type type)
		{
			return type == typeof(Sprite) || type == typeof(Texture2D);
		}

		/// <summary>
		/// Displays the Multiple Attributes Error in the console
		/// </summary>
		/// <param name="type">The type of ScriptableObject which script contains the error.</param>
		private static void MultipleAttributesError (Type type)
		{
			string message = "The type " + type.Name + " has multiple AssetIcon attributes.\n" +
				"Please ensure there is only 1, since AssetIcon won't know which one to use.";

			Error (type, message);
		}

		/// <summary>
		/// Displays the Unsupported Type Error in the console
		/// </summary>
		/// <param name="type">The type of ScriptableObject which script contains the error.</param>
		private static void UnsupportedTypeError (Type type)
		{
			string message = "The type " + type.Name + " has an AssetIcon attribute on " +
				"a type which is not supported by AssetIcon.\n" +
				"Please use it on a Sprite or a Texture type field.";

			Error (type, message);
		}

		/// <summary>
		/// Displays the Unsupported Parameters Error in the console
		/// </summary>
		/// <param name="type">The type of ScriptableObject which script contains the error.</param>
		private static void UnsupportedParametersError (Type type)
		{
			string message = "The type " + type.Name + " has an AssetIcon attribute on " +
				"a method with parameters.\n" +
				"Please use it on methods with no parameters";

			Error (type, message);
		}

		/// <summary>
		/// Logs and error and finds the first use of the AssetIcon attribute and links that script
		/// in the console.
		/// </summary>
		/// <param name="type">The type of ScriptableObject which script contains the error.</param>
		/// <param name="message">The message to display in the console.</param>
		private static void Error (Type type, string message)
		{
			ScriptableObject temporaryInstance = (ScriptableObject)Activator.CreateInstance (type);
			MonoScript errorScript = MonoScript.FromScriptableObject (temporaryInstance);
			string errorPath = AssetDatabase.GetAssetPath (errorScript);

			string errorScriptContent = errorScript.text;

			int errorLine;
			int errorCollum;

			GetLineOfAttribute (errorScriptContent, "AssetIcon", out errorLine, out errorCollum);

			message = errorPath + "(" + errorLine.ToString () + "," + errorCollum.ToString () + "): " + message;

			try
			{
				MethodInfo unityLog = typeof (UnityEngine.Debug).GetMethod ("LogPlayerBuildError", BindingFlags.NonPublic | BindingFlags.Static);

				unityLog.Invoke(null, new object[] { message, errorPath, errorLine, errorCollum });
			}
			catch
			{
				Debug.LogError (message, errorScript);
			}
		}

		/// <summary>
		/// Gets the location inside a script of the first occouring attribute with the passed name.
		/// </summary>
		/// <param name="source">The scripts content.</param>
		/// <param name="attribute">The attribute to look for (without Attribute on the end).</param>
		/// <param name="line">The line that the attribute was found on.</param>
		/// <param name="collum">The collum the attribute begins in.</param>
		private static void GetLineOfAttribute (string source, string attribute, out int line, out int collum)
		{
			//Remove Comments and replace them with empty lines
			//Old Regex: (\/\*[\s\S]*?\*\/|\/\/[\s\S]*?(\n|\r))
			string withoutComments = Regex.Replace (source,
				@"'[\s\S]*?'|""[\s\S]*?""|\/\*[\s\S]*?\*\/|\/\/[\s\S]*?(\n|\r)+", ReplaceWithEquivalentNewlines);

			//Search for the attribute
			Match match = Regex.Match (withoutComments, @"\[[\s]*" + attribute + @"([\s])*((\(+)[\s\S]*?]|])");

			//Look at the script leading up to the attribute
			string leadingToString = withoutComments.Substring (0, match.Index + match.Length);

			//Seperate the script into lines
			string[] lines = leadingToString.Split (newLineSeperators, StringSplitOptions.None);

			line = lines.Length;
			collum = lines [lines.Length - 1].Length;
		}

		/// <summary>
		/// Removes all characters in a string apart from the newline operators.
		/// </summary>
		/// <returns>The source without any other characters than newlines.</returns>
		/// <param name="match">The source.</param>
		private static string ReplaceWithEquivalentNewlines (Match match)
		{
			return ReplaceWithEquivalentNewlines (match.ToString ());
		}

		/// <summary>
		/// Removes all characters in a string apart from the newline operators.
		/// </summary>
		/// <returns>The source without any other characters than newlines.</returns>
		/// <param name="source">The source.</param>
		private static string ReplaceWithEquivalentNewlines (string source)
		{
			int newlines = CountNewlines (source);

			string newlineOperators = "";

			for (int i = 0; i < newlines - 1; i++)
			{
				newlineOperators += "\n";
			}

			return newlineOperators;
		}

		/// <summary>
		/// Returns the amount of newline operators in a source
		/// </summary>
		/// <returns>The amount of newlines in the source.</returns>
		/// <param name="source">The source.</param>
		private static int CountNewlines (string source)
		{
			return source.Split (newLineSeperators, StringSplitOptions.None).Length;
		}
	}
}
#endif