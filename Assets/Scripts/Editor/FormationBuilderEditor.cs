using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEditor;
using FlavBattle.Formation;

[CustomEditor(typeof(FormationBuilder))]
public class FormationBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate"))
        {
            var builder = (FormationBuilder)this.target;
            builder.TestBuild();
        }
    }
}
