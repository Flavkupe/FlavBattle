﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ClipperWindow : EditorWindow
{
    private Rect _imgRect;
    private Rect _newImgRect;
    private Rect _scrollAreaRect;
    private Texture2D _newTexture;

    private Texture2D _sourceTexture;
    private int _selectionWidth = 32;
    private int _selectionHeight = 32;

    private int _textureCount = 5;
    private int _prevTextureCount = 5;
    private int _currentTexture = 0;
    private int _xOffset = 0;
    private int _yOffset = 0;

    private string _currentPath;

    private int _paddingLeft = 5;
    private int _controlHeight = 15;

    private Vector2 _scrollPos = Vector2.zero;
    private Vector2 _scrollPrev = Vector2.zero;

    private ImageEncoding _encoding = ImageEncoding.Png;

    private enum ImageEncoding
    {
        Png = 0,
        Jpg = 1,
        Tga = 2,
    }

    [MenuItem("Assets/Utils/Clipper")]
    public static void OpenClipper()
    {
        var selected = Selection.activeObject as Texture2D;
        if (selected != null)
        {
            var win = EditorWindow.GetWindow<ClipperWindow>();
            win.Init(selected);
        }
        else
        {
            Debug.LogWarning("Must select a Texture2D object!");
        }
    }

    private void Init(Texture2D selected)
    {
        _currentPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(selected));

        // _imgRect is relative to scrollbar
        _imgRect = new Rect(0, 0, selected.width, selected.height);
        _sourceTexture = selected;
        _newTexture = new Texture2D(_selectionWidth * _textureCount, _selectionHeight);
        _currentTexture = 0;
        _scrollPos = Vector2.zero;
        _scrollPrev = Vector2.zero;
        _prevTextureCount = _textureCount;
    }

    void OnGUI()
    {
        if (_sourceTexture)
        {
            if (!_sourceTexture.isReadable)
            {
                EditorGUI.LabelField(new Rect(5, 5 , 300, 15), "Texture is not readable! Toggle Read/Write Enabled");
                EditorGUI.LabelField(new Rect(5, 20, 300, 15), "on image temporarily to use this, then reopen.");
                return;
            }

            wantsMouseMove = true;

            if (Event.current.type == EventType.MouseUp)
            {
                if (TryTransferImage())
                {
                    Repaint();
                    return;
                }
            }

            var widthLabel = new Rect(_paddingLeft, 5, 50, _controlHeight);
            var heightLabel = Underneath(widthLabel, 5, 50, _controlHeight);

            _selectionWidth = DrawLabelAndInput(widthLabel, "Width", _selectionWidth, 1, (int)_imgRect.width);
            _selectionHeight = DrawLabelAndInput(heightLabel, "Height", _selectionHeight, 1, (int)_imgRect.width);

            var xOffsetLabel = ToRightOf(widthLabel, 100, 50, _controlHeight);
            var yOffsetLabel = Underneath(xOffsetLabel, 5, 50, _controlHeight);
            _xOffset = DrawLabelAndInput(xOffsetLabel, "X Offset", _xOffset, 0, (int)_imgRect.width);
            _yOffset = DrawLabelAndInput(yOffsetLabel, "Y Offset", _yOffset, 0, (int)_imgRect.height);

            var columnsLabel = ToRightOf(xOffsetLabel, 100, 50, _controlHeight);
            _textureCount = DrawLabelAndInput(columnsLabel, "Num Textures", _textureCount, 2, 20);
            if (_textureCount != _prevTextureCount)
            {
                Reset();
            }

            _encoding = (ImageEncoding)EditorGUI.EnumPopup(Underneath(columnsLabel, 5, 80, _controlHeight), _encoding);

            var resetButton = ToRightOf(columnsLabel, 100, 50, _controlHeight);
            if (GUI.Button(resetButton, "Reset"))
            {
                Reset();
            }

            var saveButton = Underneath(resetButton, 5, 50, _controlHeight);
            if (GUI.Button(saveButton, "Save"))
            {
                SaveToImage();
                this.Close();
            }

            _newImgRect = Underneath(heightLabel, 15, _selectionWidth * _textureCount, _selectionHeight);
            EditorGUI.DrawTextureTransparent(_newImgRect, _newTexture);

            _scrollAreaRect = Underneath(_newImgRect, 15, 500, 500);
            var scrollViewport = new Rect(0, 0, _imgRect.width + _selectionWidth, _imgRect.height + _selectionHeight);

            // Adjust scrollbar
            _scrollPos = AdjustScrollbar(_selectionWidth, _selectionHeight);
            _scrollPrev = _scrollPos;
            _scrollPos = GUI.BeginScrollView(_scrollAreaRect, _scrollPos, scrollViewport, false, false);
            

            EditorGUI.DrawTextureTransparent(_imgRect, _sourceTexture);

            var imgCursor = this.GetMouseSelectionFromTarget(_imgRect);
            if (imgCursor != null)
            {
                EditorGUI.DrawRect(imgCursor.Value, Color.red.SetAlpha(0.3f));
            }

            GUI.EndScrollView(true);

            if (Event.current.type == EventType.MouseMove)
            {
                Repaint();
            }
        }
    }

    private void Reset()
    {
        Init(_sourceTexture);
    }

    private int DrawLabelAndInput(Rect labelRect, string labelText, int inputValue, int valueMin, int valueMax)
    {
        EditorGUI.LabelField(labelRect, labelText);
        return (int)Mathf.Clamp(EditorGUI.IntField(ToRightOf(labelRect, 5, 30, _controlHeight), inputValue), valueMin, valueMax);
    }

    private Vector2 AdjustScrollbar(int xIncrement, int yIncrement)
    {
        if (_scrollPos.x > _scrollPrev.x)
        {
            _scrollPos.x = _scrollPrev.x + xIncrement;
        }
        else if (_scrollPos.y > _scrollPrev.y)
        {
            _scrollPos.y = _scrollPrev.y + xIncrement;
        }
        else if (_scrollPos.x < _scrollPrev.x)
        {
            _scrollPos.x = _scrollPrev.x - yIncrement;
        }
        else if (_scrollPos.y < _scrollPrev.y)
        {
            _scrollPos.y = _scrollPrev.y - yIncrement;
        }

        return _scrollPos;
    }

    private bool TryTransferImage()
    {
        if (_currentTexture == _textureCount)
        {
            return false;
        }

        var cursorVal = this.GetMouseSelectionFromTarget(_scrollAreaRect, false);
        if (!cursorVal.HasValue)
        {
            return false;
        }

        var cursor = cursorVal.Value;

        var x = (int)cursor.x;
        var y = (int)cursor.y;
        var width = (int)cursor.width;
        var height = (int)cursor.height;

        // Adjust for scroll
        var scrollAdjustX = SnapToNearestFactor((int)_scrollPos.x, _selectionWidth);
        var scrollAdjustY = SnapToNearestFactor((int)_scrollPos.y, _selectionHeight);
        x += scrollAdjustX;
        y += scrollAdjustY;

        var workingHeight = FloorToFactor((int)_imgRect.height, _selectionHeight);
        var lowestY = _imgRect.height - workingHeight;

        // This one is a little tricky: While Rect is top-to-bottom, GetPixel is bottom-to-top (ughh...).
        // So we want to invert the y (workingHeight - y) and shift by the lowestY, which is the bottom
        // strip that we ignore because it's not of the right dimensions. Finally, we shift down by the selection
        // height (_selectionHeight) because we start gathering pixels from bottom to top.
        var effectiveY = (int)lowestY + workingHeight - y +  - _selectionHeight;
        var pixels = _sourceTexture.GetPixels(x, effectiveY, width, height);

        var currentTextureX = _currentTexture * _selectionWidth;
        
        _newTexture.SetPixels(currentTextureX, 0, width, height, pixels);
        _newTexture.Apply();
        _currentTexture++;
        return true;
    }

    private Rect? GetMouseSelectionFromTarget(Rect target, bool global = true)
    {
        if (!target.Contains(Event.current.mousePosition))
        {
            return null;
        }
        
        var x = (int)(Event.current.mousePosition.x - target.x);
        var y = (int)(Event.current.mousePosition.y - target.y);
        x = FloorToFactor(x, _selectionWidth);
        y = FloorToFactor(y, _selectionHeight);
        x += _xOffset;
        y += _yOffset;
        if (x + _selectionWidth > target.width ||
            y + _selectionHeight > target.height)
        {
            // flows over edge
            return null;
        }

        var finalX = x + (global ? target.x : 0);
        var finalY = y + (global ? target.y : 0);
        return new Rect(finalX, finalY, _selectionWidth, _selectionHeight);
    }

    private void SaveToImage()
    {
        byte[] bytes;
        string ext;
        switch (_encoding)
        {
            case ImageEncoding.Jpg:
                bytes = _newTexture.EncodeToJPG();
                ext = "jpg";
                break;
            case ImageEncoding.Tga:
                bytes = _newTexture.EncodeToTGA();
                ext = "tga";
                break;
            case ImageEncoding.Png:
            default:
                bytes = _newTexture.EncodeToPNG();
                ext = "png";
                break;
        }

        File.WriteAllBytes(Path.Combine(_currentPath, $"NewFile.{ext}"), bytes);
        AssetDatabase.Refresh();
    }

    private Rect ToRightOf(Rect rect, int padding, int width, int height)
    {
        return new Rect(rect.x + rect.width + padding, rect.y, width, height);
    }

    private Rect Underneath(Rect rect, int padding, int width, int height)
    {
        return new Rect(rect.x, rect.y + rect.height + padding, width, height);
    }

    /// <summary>
    /// Goes down to nearest factor of value. If value
    /// is 20 and factor is 16, will return 16. If value is
    /// 35 and factor is 16, will return 32, etc
    /// </summary>
    private int FloorToFactor(int value, int factor)
    {
        return value = (value / factor) * factor;
    }

    private int SnapToNearestFactor(int value, int factor)
    {
        var floor = (value / factor) * factor;
        var roof = ((value / factor) + 1) * factor;
        return Math.Abs(value - floor) > Math.Abs(value - roof) ? roof : floor;
    }
}
