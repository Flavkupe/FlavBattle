
using FlavBattle.Resources;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System;

[CustomPropertyDrawer(typeof(StringResource))]
public class StringResourceAttributeDrawer : PropertyDrawer
{
    private static StringResourceMap _resources = null;

    private float _fieldHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

    private string _currentText = null;

    private int _indent = 8;
    private int _padding = 4;

    private bool _toggle = false;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return _toggle ? _fieldHeight * 4 : _fieldHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (_resources == null)
        {
            this.Refresh();
        }

        var rectHeight = GetPropertyHeight(property, label);

        EditorGUI.BeginProperty(position, label, property);

        var foldoutPos = new Rect(position.x, position.y, position.width, _fieldHeight);
        
        _toggle = EditorGUI.Foldout(foldoutPos, _toggle, label, true);

        // var position = new Rect(position.x + _indent, position.y, position.width, rectHeight);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // get actual values of props
        var categoryProp = property.FindPropertyRelative(nameof(StringResource.Category));
        var keyProp = property.FindPropertyRelative(nameof(StringResource.Key));

        if (_toggle)
        {
            RenderExpandedView(position, categoryProp, keyProp);
        }
        else
        {
            RenderSimplifiedView(position, categoryProp, keyProp);
        }

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    private StringResourceCategory GetCategoryFromProp(SerializedProperty categoryProp)
    {
        var currentCategory = StringResourceCategory.General;
        if (categoryProp.enumValueIndex != -1)
        {
            currentCategory = (StringResourceCategory)categoryProp.enumValueIndex;
        }

        return currentCategory;
    }

    private void RenderSimplifiedView(Rect position, SerializedProperty categoryProp, SerializedProperty keyProp)
    {
        var currentKey = keyProp.stringValue;
        var currentCategory = GetCategoryFromProp(categoryProp);
        _currentText = string.Empty;
        if (!string.IsNullOrEmpty(currentKey))
        {
            _currentText = _resources.Get(currentCategory, currentKey) ?? string.Empty;
        }

        var width = position.width;
        var labelWidth = EditorGUIUtility.labelWidth;

        var textRect = new Rect(position.x + labelWidth, position.y, width - labelWidth, _fieldHeight);

        _currentText = EditorGUI.TextArea(textRect, _currentText);
    }


    private void RenderExpandedView(Rect position, SerializedProperty categoryProp, SerializedProperty keyProp)
    {
        var currentKey = keyProp.stringValue;
        var currentCategory = GetCategoryFromProp(categoryProp);

        var valueChanged = false;

        // width for controls, based on original allocation
        var width = position.width - _indent;
        var buttonWidth = Math.Min(60.0f, width / 4.0f);
        var widthMinusButton = width - buttonWidth;

        var categoryRect = new Rect(position.x, position.y + _fieldHeight, widthMinusButton - _padding, _fieldHeight);
        var buttonRect = new Rect(position.x + widthMinusButton, position.y + _fieldHeight, buttonWidth, _fieldHeight - 2);
        var keyRect = new Rect(position.x, position.y + (2 * _fieldHeight), width, _fieldHeight);
        var textRect = new Rect(position.x, position.y + (3 * _fieldHeight), widthMinusButton - _padding, _fieldHeight);
        var updateTextRect = new Rect(position.x + widthMinusButton, position.y + (3 * _fieldHeight), buttonWidth, _fieldHeight - 2);

        currentCategory = (StringResourceCategory)EditorGUI.EnumPopup(categoryRect, currentCategory);

        if (categoryProp.enumValueIndex != (int)currentCategory)
        {
            valueChanged = true;
            categoryProp.enumValueIndex = (int)currentCategory;
        }

        if (GUI.Button(buttonRect, "Refresh"))
        {
            this.Refresh();
            valueChanged = true;
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
            ChangeString(currentCategory, currentKey);
            return;
        }
    }

    private void Refresh()
    {
        _resources = StringResources.GetStrings();
    }

    private void ChangeString(StringResourceCategory category, string key)
    {
        ResourceUtils.WriteToFile(_resources, category, key, _currentText);
    }
}
