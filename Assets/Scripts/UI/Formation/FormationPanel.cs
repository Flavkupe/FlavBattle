using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class FormationPanel : MonoBehaviour
{
    private UIFormationGridTile[] _tiles;

    public UIFormationGrid Grid;

    // Start is called before the first frame update
    void Awake()
    {
        FormationUtils.PopulateFormationGrid(Grid, FormationOrientation.BottomRight, 50.0f);
        this._tiles = Grid.Slots.ToArray();
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

            var tile = this._tiles.FirstOrDefault(item => item.Row == unit.Formation.Row && item.Col == unit.Formation.Col);
            Debug.Assert(tile != null, "No tile found for unit formation");
            if (tile != null)
            {
                tile.SetUnit(unit);
            }
        }
    }
}
