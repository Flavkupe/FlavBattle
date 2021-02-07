using System.Collections;
using System.Collections.Generic;
using TMPro;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using FlavBattle.UI.Army;

public class UnitStatsPanel : MonoBehaviour
{
    private enum ExtraPanelType
    {
        OfficerCommands,
        Actions,
        Perks,
    }

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

    [BoxGroup("Panels")]
    [Required]
    public GameObject ExtraInfoPanel;

    [BoxGroup("Panels")]
    [Required]
    [SerializeField]
    private OfficerCommandsPanel _officerCommandsPanel;

    [BoxGroup("Panels")]
    [Tooltip("The layout panel that holds individual action info panels")]
    [Required]
    [SerializeField]
    private UICombatActionInfoPanel _attackActionInfoPanel;


    [BoxGroup("Panels")]
    [Tooltip("The panel for showing perks.")]
    [Required]
    [SerializeField]
    private UIUnitPerksPanel _unitPerksPanel;

    

    private List<GameObject> _extraPanels = new List<GameObject>();

    void Start()
    {
        _extraPanels.Add(_officerCommandsPanel.gameObject);
        _extraPanels.Add(_attackActionInfoPanel.gameObject);
        _extraPanels.Add(_unitPerksPanel.gameObject);
        HideExtraPanels();
        ExtraInfoPanel.Hide();
    }

    public void ClearUnit()
    {
        _attackActionInfoPanel.ClearUnit();
    }


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

        this._officerCommandsPanel.SetUnit(unit);
        this.CommandsButton.interactable = unit.IsOfficer;

        _unitPerksPanel.SetUnit(unit);

        if (unit == null)
        {
            _attackActionInfoPanel.ClearUnit();
        }
        else
        {
            _attackActionInfoPanel.SetUnit(unit);
        }

    }

    public void CloseExtraPanel()
    {
        ExtraInfoPanel.Hide();
    }

    public void ToggleOfficerPanel()
    {
        ToggleExtraPanel(ExtraPanelType.OfficerCommands);
    }

    public void ToggleActionsPanel()
    {
        ToggleExtraPanel(ExtraPanelType.Actions);
    }

    public void TogglePerksPanel()
    {
        ToggleExtraPanel(ExtraPanelType.Perks);
    }

    private void ToggleExtraPanel(ExtraPanelType panel)
    {
        GameObject panelToToggle = null;

        switch (panel)
        {
            case ExtraPanelType.OfficerCommands:
                panelToToggle = _officerCommandsPanel.gameObject;
                break;
            case ExtraPanelType.Actions:
                panelToToggle = _attackActionInfoPanel.gameObject;
                break;
            case ExtraPanelType.Perks:
                panelToToggle = _unitPerksPanel.gameObject;
                break;
            default:
                break;
        }

        if (panelToToggle == null)
        {
            Debug.LogWarning($"No panel matching type to open: {panel}");
            return;
        }

        if (panelToToggle.activeInHierarchy)
        {
            // toggle off
            panelToToggle.Hide();
            ExtraInfoPanel.Hide();
            Sounds.Play(UISoundType.Close);
        }
        else
        {
            ExtraInfoPanel.Show();
            // hide all panels then open this one

            HideExtraPanels();
            panelToToggle.Show();
            Sounds.Play(UISoundType.Open);
        }
    }

    private void HideExtraPanels()
    {
        foreach (var item in _extraPanels)
        {
            item.Hide();
        }
    }
}
