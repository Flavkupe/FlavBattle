using FlavBattle.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class StringResourceWindow : EditorWindow
{
    private static StringResourceMap _resources = null;
    private StringResourceCategory _category;
    private string _key;
    private int _dropdownKeyIndex = 0;
    private string _text;
    private string _originalText;

    [MenuItem("Window/String Resource Window")]
    public static void OpenWindow()
    {
        EditorWindow.GetWindow<StringResourceWindow>();
    }

    private void OnGUI()
    {
        if (_resources == null)
        {
            this.Refresh();
        }

        EditorGUILayout.BeginHorizontal();

        var changed = false;
        var category = (StringResourceCategory)EditorGUILayout.EnumPopup("Category", _category);
        if (category != _category)
        {
            changed = true;
            _category = category;
            _dropdownKeyIndex = 0;
        }

        var buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fixedWidth = 60.0f;

        if (GUILayout.Button("Refresh", buttonStyle))
        {
            this.Refresh();
            return;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        // Key dropdown, used only to help with inputs
        var keys = _resources.GetKeys(category).ToArray();
        var dropdownKeyIndex = EditorGUILayout.Popup("ID", _dropdownKeyIndex, keys);
        if (dropdownKeyIndex != -1 && keys.Length > dropdownKeyIndex && 
            (_dropdownKeyIndex != dropdownKeyIndex || changed))
        {
            changed = true;
            _key = keys[dropdownKeyIndex];
            _dropdownKeyIndex = dropdownKeyIndex;
        }

        // Free-form key input
        var key = EditorGUILayout.TextField(_key);
        if (key != _key)
        {
            changed = true;
            _key = key;
        }

        var keyExists = _resources.KeyExists(_category, _key);
        if (changed && keyExists)
        {
            _originalText = _resources.Get(_category, _key);
            _text = _originalText;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        _text = EditorGUILayout.TextField("Text", _text);
        GUI.enabled = !keyExists || _originalText != _text;

        var updateButtonText = keyExists ? "Change" : "Add";

        if (GUILayout.Button(updateButtonText, buttonStyle))
        {
            ChangeString(_category, _key, _text);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void Refresh()
    {
        _resources = StringResources.GetStrings(true);
    }

    private void ChangeString(StringResourceCategory category, string key, string text)
    {
        ResourceUtils.WriteToFile(_resources, category, key, text);
        _originalText = text;
    }
}
