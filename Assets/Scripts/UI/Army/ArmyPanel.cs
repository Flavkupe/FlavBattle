using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ArmyPanel : MonoBehaviour
{
    public UIFormationGrid ArmyGridTemplate;

    private List<Army> _playerArmies = new List<Army>();

    private GameEventManager _gameEvents;

    private List<UIFormationGrid> _grids = new List<UIFormationGrid>();

    public event EventHandler<Army> ArmyClicked;

    public event EventHandler<Army> ArmyEditRequested;

    public GameObject ScrollContent;

    public void UpdatePanelContents()
    {
        foreach (var grid in _grids)
        {
            grid.UpdateArmy();
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        _gameEvents = FindObjectOfType<GameEventManager>();
        _gameEvents.ArmyCreatedEvent += HandleArmyCreatedEvent;
    }

    private void HandleArmyCreatedEvent(object sender, Army e)
    {
        if (e.IsPlayerArmy)
        {
            _playerArmies.Add(e);
            var grid = FormationUtils.CreateFormationGrid(ArmyGridTemplate, FormationOrientation.BottomRight, 50.0f);
            grid.transform.SetParent(ScrollContent.transform);
            grid.SetArmy(e);
            grid.GridClicked += HandleGridClicked;
            grid.GridRightClicked += HandleGridRightClicked;
            _grids.Add(grid);
        }
    }

    private void HandleGridRightClicked(object sender, UIFormationGrid grid)
    {
        ArmyEditRequested?.Invoke(this, grid.Army);
    }

    private void HandleGridClicked(object source, UIFormationGrid grid)
    {
        ArmyClicked?.Invoke(this, grid.Army);
    }
}
