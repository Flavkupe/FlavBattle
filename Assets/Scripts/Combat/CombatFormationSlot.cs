using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The formation slot during combat for a single unit. Contains the unit, unless empty.
/// </summary>
public class CombatFormationSlot : MonoBehaviour, IFormationGridSlot
{
    public CombatUnit CombatUnitTemplate;

    public bool FacingLeft { get; set; }

    [SerializeField]
    private float _yOffset = 0.25f;

    public FormationRow Row { get; set; }
    public FormationColumn Col { get; set; }

    public CombatUnit CurrentUnit { get; private set; }

    public MonoBehaviour Instance => this;

    private SpriteRenderer _sprite;
    private Color _startColor;

    [Required]
    public SpriteRenderer HighlightRing;

    [Required]
    public GameObject OfficerRing;

    public FormationPair GetFormation()
    {
        return new FormationPair
        {
            Row = this.Row,
            Col = this.Col
        };
    }

    private void Start()
    {
        OfficerRing.Hide();
        _sprite = GetComponent<SpriteRenderer>();
        if (_sprite != null)
        {
            _startColor = _sprite.color;
        }
    }

    public void SetUnit(Unit unit)
    {
        // Put it in the .25f mark, adjusting for scale of slot
        var yPos = _yOffset / transform.localScale.y;
        var combatUnit = Instantiate(CombatUnitTemplate);
        combatUnit.name = unit.Info.Name;
        combatUnit.SetUnit(unit, FacingLeft);
        CurrentUnit = combatUnit;
        combatUnit.transform.SetParent(this.transform);
        combatUnit.transform.localPosition = new Vector3(0.0f, yPos, 0.0f);

        if (unit.IsOfficer)
        {
            OfficerRing.Show();
        }
        else
        {
            OfficerRing.Hide();
        }
    }

    public void ClearContents()
    {
        if (CurrentUnit != null)
        {
            Destroy(CurrentUnit.gameObject);
            CurrentUnit = null;
        }
    }

    public void Highlight(Color color)
    {
        if (HighlightRing != null)
        {
            HighlightRing.gameObject.Show();
            HighlightRing.color = color.SetAlpha(0.5f);
        }
    }

    public void ResetColor()
    {
        if (HighlightRing != null)
        {
            HighlightRing.color = _startColor;
            HighlightRing.gameObject.Hide();
        }
    }
}
