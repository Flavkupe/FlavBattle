using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FormationPanelTile : MonoBehaviour
{
    public FormationColumn Column;
    public FormationRow Row;

    public Image UnitImage;

    private Unit _unit;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
