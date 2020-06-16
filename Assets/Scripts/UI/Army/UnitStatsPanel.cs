using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatsPanel : MonoBehaviour
{
    public FacePortrait Portrait;
    public NameTag NameTag;

    public IconLabelPair Level;
    public IconLabelPair Class;
    public IconLabelPair HP;
    public IconLabelPair Power;
    public IconLabelPair Def;

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
    }
}
