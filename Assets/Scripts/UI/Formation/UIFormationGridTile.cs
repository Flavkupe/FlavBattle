using FlavBattle.Combat;
using FlavBattle.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This is the non-interactive UIFormationGridTile
/// </summary>
[RequireComponent(typeof(Image))]
public class UIFormationGridTile : MonoBehaviour, IFormationGridSlot
{
    public MonoBehaviour Instance => this;

    public FormationRow Row { get { return _row; } set { _row = value; } }
    public FormationColumn Col { get { return _col; } set { _col = value; } }

    [SerializeField]
    public FormationRow _row;

    [SerializeField]
    public FormationColumn _col;

    private AnimatedCharacter _character;

    [SerializeField]
    private Vector3 _characterOffset = new Vector3(0.0f, 0.0f, -1.0f);

    [SerializeField]
    private Vector3 _characterScale = new Vector3(32.0f, 32.0f, 1.0f);

    [SerializeField]
    private SortingLayerValues _sortingLayer;

    private Unit _unit;

    private void Start()
    {
        
    }

    public void SetColor(Color color)
    {
        if (_character != null)
        {
            _character.SetColor(color);
        }
    }

    public void SetUnit(Unit unit)
    {
        this._unit = unit;
        if (unit == null)
        {
            if (_character != null)
            {
                Destroy(_character.gameObject);
                _character = null;
            }
        }
        else
        {
            _character = Instantiate(unit.Data.AnimatedCharacter, this.transform, false);
            _character.transform.localPosition = _characterOffset;
            _character.transform.localScale = _characterScale;
            _character.PlayAnimation(UnitAnimatorTrigger.Static);
            _character.SetSortingLayer(_sortingLayer.Name, _sortingLayer.Value);
        }
    }
}
