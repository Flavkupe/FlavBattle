using FlavBattle.State;
using FlavBattle.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class TraceWindow : EditorWindow
{
    private const int LEFT_INDENT = 1;

    /// <summary>
    /// Max recursive depth for traces
    /// </summary>
    private const int MAX_DEPTH = 6;

    private List<TraceData> _traces = new List<TraceData>();

    [MenuItem("Window/Debug Utils/Open Trace Window")]
    public static void OpenWindow()
    {
        var win = EditorWindow.GetWindow<TraceWindow>();
    }

    private HashSet<string> _expanded = new HashSet<string>();

    private void OnGUI()
    {
        if (GUILayout.Button("Get Trace"))
        {
            GetTrace();       
        }

        foreach (var trace in _traces)
        {
            TraverseTrace(trace);
        }
    }

    private void TraverseTrace(TraceData trace, string parentKey = null, int depth = 0)
    {
        if (depth >= MAX_DEPTH)
        {
            // failsafe for recursion
            return;
        }

        if (trace.Data.Count == 0)
        {
            DrawLabel(trace, depth);
        }
        else
        {
            DrawFoldout(trace, parentKey, depth);
        }
    }

    private void DrawLabel(TraceData trace, int depth = 0)
    {
        var style = new GUIStyle(GUI.skin.label);
        EditorGUI.indentLevel = depth * LEFT_INDENT;

        if (trace.Context != null)
        {
            EditorGUILayout.BeginHorizontal();
        }

        if (trace.Detail != null)
        {
            style.richText = true;
            EditorGUILayout.LabelField($"<b>{trace.Name}:</b> {trace.Detail}", style);
        }
        else
        {
            EditorGUILayout.LabelField(trace.Name, style);
        }

        if (trace.Context != null)
        {
            EditorGUILayout.ObjectField(trace.Context, typeof(GameObject), true);
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawFoldout(TraceData trace, string parentKey = null, int depth = 0)
    {
        var identifier = trace.Key ?? trace.Name;
        var key = parentKey != null ? $"{parentKey}_{identifier}" : identifier;

        EditorGUI.indentLevel = depth * LEFT_INDENT;

        if (trace.Context != null)
        {
            EditorGUILayout.BeginHorizontal();
        }

        var expanded = _expanded.Contains(key);
        expanded = EditorGUILayout.Foldout(expanded, trace.Name);

        if (trace.Context != null)
        {
            EditorGUILayout.ObjectField(trace.Context, typeof(GameObject), true);
            EditorGUILayout.EndHorizontal();
        }

        if (expanded)
        {
            _expanded.Add(key);
        }
        else if (_expanded.Contains(key))
        {
            _expanded.Remove(key);
        }

        if (expanded)
        {
            foreach (var item in trace.Data)
            {
                TraverseTrace(item, key, depth + 1);
            }
        }
    }

    private void GetTrace()
    {
        _traces.Clear();
        var items = MiscUtils.FindOfType<IHasTraceData>();
        foreach (var item in items)
        {
            var trace = item.GetTrace();
            if (trace != null && trace.IsTopLevel)
            {
                _traces.Add(trace);
            }
        }
    }
}
