using System.Collections;
using System.Collections.Generic;
using TMPro;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatsPanel : MonoBehaviour
{
    [BoxGroup("Visual")]
    public IconLabelPair Level;

    [BoxGroup("Visual")]
    public IconLabelPair Class;

    [BoxGroup("Visual")]
    public IconLabelPair HP;

    [BoxGroup("Visual")]
    public IconLabelPair Power;

    [BoxGroup("Visual")]
    public IconLabelPair Def;

    [BoxGroup("Buttons")]
    public Button CommandsButton;

    [BoxGroup("Other")]
    public FacePortrait Portrait;

    [BoxGroup("Other")]
    public NameTag NameTag;

    [BoxGroup("Other")]
    public OfficerCommandsPanel OfficerCommandsPanel;

    public void SetUnit(Unit unit)
    {
        this.Portrait.SetUnit(unit);
        this.Class.SetIcon(unit.Data.Icon);
        this.Class.SetText(unit.Data.ClassName);
        this.Level.SetText($"Level {unit.Info.MaxStats.Level}");
        this.HP.SetText($"{unit.Info.CurrentStats.HP} / {unit.Info.MaxStats.HP}");
        this.Power.SetText($"{unit.Info.MaxStats.Power}");
        this.Def.SetText($"{unit.Info.MaxStats.Defense}");
        this.NameTag.SetUnit(unit);

        this.OfficerCommandsPanel.SetUnit(unit);
        this.OfficerCommandsPanel.Hide();
        this.CommandsButton.interactable = unit.IsOfficer;
        
    }
}
