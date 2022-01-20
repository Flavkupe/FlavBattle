using FlavBattle.Entities;
using FlavBattle.UI;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionButtonsPanel : MonoBehaviour
{
    private ArmyActions _armyActions;

    [SerializeField]
    [Required]
    private GameObject _actionsPanel;

    [SerializeField]
    [Required]
    private ArmyActionButton _buttonTemplate;

    private List<Tuple<IArmyAction, ArmyActionButton>> _actions = new List<Tuple<IArmyAction, ArmyActionButton>>();

    private void Update()
    {
        if (_armyActions != null)
        {
            foreach (var action in _actions)
            {
                var current = action.Item1;
                var button = action.Item2;
                button.SetActive(current.IsAvailable());
                button.SetLocked(current.IsLocked());
            }
        }
    }

    public void SetArmy(Army army)
    {
        if (army == null)
        {
            this.UnsetArmyActions();
        }
        else
        {
            var actions = army.GetComponentInChildren<ArmyActions>();
            this.SetArmyActions(actions);
        }
    }

    public void SetArmyActions(ArmyActions selected)
    {
        if (_armyActions == selected)
        {
            return;
        }

        _armyActions = selected;
        Clear();
        foreach (var action in _armyActions.Actions)
        {
            var button = Instantiate(_buttonTemplate, this._actionsPanel.transform, false);
            button.SetAction(action);
            _actions.Add(new Tuple<IArmyAction, ArmyActionButton>(action, button));
        }
    }

    public void UnsetArmyActions()
    {
        _armyActions = null;
        Clear();
    }

    private void Clear()
    {
        _actionsPanel.transform.DestroyChildren();
        _actions.Clear();

    }
}
