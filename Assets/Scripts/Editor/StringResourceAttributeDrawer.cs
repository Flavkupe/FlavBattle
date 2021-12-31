
using FlavBattle.Resources;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;

[CustomPropertyDrawer(typeof(StringResource))]
public class StringResourceAttributeDrawer : PropertyDrawer
{
    private StringResourceMap _resources = null;

    private float _fieldHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

    private string _currentText = null;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return _fieldHeight * 4;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var valueChanged = false;

        // width for controls, based on original allocation
        var width = position.width / 2.0f;
        var halfWidth = width / 2.0f;

        if (_resources == null)
        {
            this.Refresh();
        }

        var rectHeight = GetPropertyHeight(property, label);
        position = new Rect(position.x, position.y, position.width, rectHeight);

        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, label);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var updateTextButtonWidth = 60;

        var categoryRect = new Rect(position.x, position.y + _fieldHeight, halfWidth, _fieldHeight);
        var buttonRect = new Rect(position.x + halfWidth + 5, position.y + _fieldHeight, 70, _fieldHeight);
        var keyRect = new Rect(position.x, position.y + (2 * _fieldHeight), width, _fieldHeight);
        var textRect = new Rect(position.x, position.y + (3 * _fieldHeight), width, _fieldHeight);
        var updateTextRect = new Rect(position.x + width, position.y + (3 * _fieldHeight), updateTextButtonWidth, _fieldHeight);

        var categoryProp = property.FindPropertyRelative(nameof(StringResource.Category));
        var keyProp = property.FindPropertyRelative(nameof(StringResource.Key));
        var currentKey = keyProp.stringValue;

        var currentCategory = StringResourceCategory.General;
        if (categoryProp.enumValueIndex != -1)
        {
            currentCategory = (StringResourceCategory)categoryProp.enumValueIndex;
        }

        currentCategory = (StringResourceCategory)EditorGUI.EnumPopup(categoryRect, currentCategory);

        if (categoryProp.enumValueIndex != (int)currentCategory)
        {
            valueChanged = true;
            categoryProp.enumValueIndex = (int)currentCategory;
        }

        if (GUI.Button(buttonRect, "Refresh"))
        {
            this.Refresh();
            return;
        }

        var keys = _resources.GetKeys(currentCategory).ToList();

        // TODO: can this be improved?
        var index = string.IsNullOrEmpty(currentKey) ? 0 : keys.IndexOf(currentKey);
        if (index == -1)
        {
            index = 0;
        }

        index = EditorGUI.Popup(keyRect, index, keys.ToArray());
        var keyValue = keys[index];
        if (keyProp.stringValue != keyValue)
        {
            valueChanged = true;
            keyProp.stringValue = keyValue;
        }

        if (valueChanged || string.IsNullOrEmpty(_currentText))
        {
            _currentText = index >= keys.Count ? string.Empty : _resources.Get(currentCategory, keyValue);
        }

        _currentText = EditorGUI.TextArea(textRect, _currentText);

        if (GUI.Button(updateTextRect, "Change"))
        {
            this.WriteToFile(currentCategory, currentKey, _currentText);
            return;
        }

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    private void WriteToFile(StringResourceCategory category, string key, string content)
    {
        var textAsset = StringResources.GetTextAsset();
        if (textAsset == null) 
        {
            Debug.LogWarning("Error getting text asset");
            return;
        }

        _resources.Set(category, key, content);
        var json = _resources.ToJSON();

        File.WriteAllText(AssetDatabase.GetAssetPath(textAsset), json);

        StringResources.UpdateStrings(_resources);
    }

    private void Refresh()
    {
        _resources = StringResources.GetStrings(true);
    }
}
