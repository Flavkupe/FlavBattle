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

    public Image UnitImage;

    private Unit _unit;

    public void SetColor(Color color)
    {
        GetComponent<Image>().color = color;
    }

    public void SetUnit(Unit unit)
    {
        this._unit = unit;
        if (unit == null)
        {
            this.UnitImage.gameObject.SetActive(false);
            this.UnitImage.sprite = null;
        }
        else
        {
            this.UnitImage.gameObject.SetActive(true);
            this.UnitImage.sprite = unit.Data.Sprite;
        }
    }
}
