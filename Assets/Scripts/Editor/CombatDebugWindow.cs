using FlavBattle.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

public class CombatDebugWindow : EditorWindow
{
    private BattleManager _manager;
    private bool _leftFoldout = false;
    private bool _rightFoldout = false;

    [MenuItem("Window/Debug Utils/Open Combat Debug Window")]
    public static void OpenWindow()
    {
        var win = EditorWindow.GetWindow<CombatDebugWindow>();
        
    }

    void Update()
    {
        if (EditorApplication.isPlaying && !EditorApplication.isPaused)
        {
            if (_manager == null)
            {
                this._manager = FindObjectOfType<BattleManager>();
            }

            if (_manager != null)
            {
                UpdateState();
                Repaint();
            }
        }
    }

    private void UpdateState()
    {
        
    }

    private void OnGUI()
    {
        if (_manager == null || !EditorApplication.isPlaying)
        {
            return;
        }

        var status = _manager.GetBattleStatus();
        var left = status.PlayerArmy;
        var right = status.OtherArmy;
        if (left == null || right == null)
        {
            return;
        }

        _leftFoldout = EditorGUILayout.Foldout(_leftFoldout, "Left");
        if (_leftFoldout)
        {
            EditorGUILayout.LabelField("Live Units:", left.Formation.GetNumberOfLiveUnits().ToString());
            EditorGUILayout.LabelField("Dead Units:", left.Formation.GetNumberOfDeadUnits().ToString());
        }

        _rightFoldout = EditorGUILayout.Foldout(_rightFoldout, "Right");
        if (_rightFoldout)
        {
            EditorGUILayout.LabelField("Live Units:", right.Formation.GetNumberOfLiveUnits().ToString());
            EditorGUILayout.LabelField("Dead Units:", right.Formation.GetNumberOfDeadUnits().ToString());
        }
    }

    private int GetNumDeadUnits(Formation formation)
    {
        return formation.GetUnits().Count(a => a != null && a.IsDead());
    }
}
