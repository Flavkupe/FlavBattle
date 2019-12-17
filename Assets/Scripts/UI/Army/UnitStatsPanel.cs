using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitStatsPanel : MonoBehaviour
{
    public FacePortrait Portrait;

    public IconLabelPair Class;
    public IconLabelPair HP;
    public IconLabelPair Power;
    public IconLabelPair Def;
    public TextMeshProUGUI NameTag;

    public void SetUnit(Unit unit)
    {
        this.Portrait.SetUnit(unit);
        this.HP.SetText($"{unit.Info.CurrentStats.HP} / {unit.Info.MaxStats.HP}");
        this.Power.SetText($"{unit.Info.MaxStats.Power}");
        this.Def.SetText($"{unit.Info.MaxStats.Defense}");
        NameTag.SetText(unit.Info.Name);
    }
}
