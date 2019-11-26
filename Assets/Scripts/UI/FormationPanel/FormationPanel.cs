using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class FormationPanel : MonoBehaviour
{
    private FormationPanelTile[] _tiles;

    // Start is called before the first frame update
    void Awake()
    {
        this._tiles = this.GetComponentsInChildren<FormationPanelTile>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClearFormation()
    {
        foreach (var tile in this._tiles)
        {
            tile.SetUnit(null);
        }
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void SetFormation(Formation formation)
    {
        ClearFormation();

        foreach (Unit unit in formation.GetUnits())
        {
            if (unit == null)
            {
                continue;
            }

            var tile = this._tiles.FirstOrDefault(item => item.Row == unit.Formation.Row && item.Column == unit.Formation.Col);
            Debug.Assert(tile != null, "No tile found for unit formation");
            if (tile != null)
            {
                tile.SetUnit(unit);
            }
        }
    }
}
