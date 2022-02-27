using FlavBattle.Core;
using FlavBattle.Formation;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using FlavBattle.Components;
using FlavBattle.Entities;

public class ArmyMapView : MonoBehaviour, IAnimatedSprite
{
    private Dictionary<FormationPair, ArmyMapViewUnit> _units = new Dictionary<FormationPair, ArmyMapViewUnit>();

    private IEnumerable<ArmyMapViewUnit> AllUnitSlots => _units.Values;

    private IEnumerable<ArmyMapViewUnit> ActiveUnits => _units.Values.Where(a => !a.IsEmpty);

    [SerializeField]
    private AnimatedCharacterVisuals _characterVisualProperties;

    private AnimatedCharacter _leaderCharacter;

    private CameraMain _cam;

    private IArmy _army;

    private bool _zoomedView = false;

    [SerializeField]
    [Required]
    private StatOverlay _overlay;

    private bool _idle = true;

    // Start is called before the first frame update
    void Start()
    {
        _cam = FindObjectOfType<CameraMain>(true);

        if (_units.Count == 0)
        {
            Init();
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            SetOverlayVisible(true);
        }
        else
        {
            SetOverlayVisible(false);
        }
    }

    private void SetOverlayVisible(bool visible)
    {
        _overlay.SetActive(visible && !_zoomedView);
        foreach (var unit in AllUnitSlots)
        {
            unit.SetOverlayVisible(visible && _zoomedView);
        }
    }

    public void SetArmy(IArmy army)
    {
        if (army == _army)
        {
            return;
        }

        if (_army != null)
        {
            _army.Formation.FormationChanged -= HandleFormationChanged;
        }

        _army = army;
        
        if (army == null)
        {
            Clear();
            return;
        }

        army.Formation.FormationChanged += HandleFormationChanged;

        if (_units.Count == 0)
        {
            Init();
        }

        SetFormation(army.Formation);

        var zoomedIn = _cam?.IsZoomedDistance() ?? false;
        SetView(zoomedIn);
    }

    private void SetFormation(Formation formation)
    {
        Clear();

        var mainUnit = formation.GetOfficer(true);
        var character = Instantiate(mainUnit.Data.AnimatedCharacter, this.transform, false);
        character.SetVisuals(this._characterVisualProperties);
        _leaderCharacter = character;

        // First hide each so only the ones in the formation
        // are later shown
        foreach (var item in AllUnitSlots)
        {
            item.Clear();
            item.Hide();
        }

        foreach (var unit in formation.GetUnits())
        {
            var slot = _units[unit.Formation];
            slot.Show();
            slot.CreateUnit(unit);
        }
    }

    private void Clear()
    {
        if (_leaderCharacter != null)
        {
            Destroy(_leaderCharacter.gameObject);
            _leaderCharacter = null;
        }

        foreach (var sprite in this.AllUnitSlots)
        {
            sprite.Clear();
        }
    }

    private void HandleFormationChanged(object obj, Formation e)
    {
        SetFormation(e);
    }

    private void Init()
    {
        Clear();
        foreach (var item in GetComponentsInChildren<ArmyMapViewUnit>(true))
        {
            var pair = item.Pair;
            _units[pair] = item;
        }
    }

    public void SetFlippedLeft(bool flipped)
    {
        _leaderCharacter.SetFlippedLeft(flipped);
        foreach (var sprite in this.AllUnitSlots)
        {
            sprite.SetFlippedLeft(flipped);
        }
    }

    public void SetIdle(bool idle)
    {
        _idle = idle;
        _leaderCharacter.SetIdle(idle);
        foreach (var sprite in this.AllUnitSlots)
        {
            sprite.SetIdle(idle);
        }
    }

    public void SetColor(Color color)
    {
        _leaderCharacter.SetColor(color);
        foreach (var sprite in this.AllUnitSlots)
        {
            sprite.SetColor(color);
        }
    }

    public void SetSpeedModifier(float modifier)
    {
        _leaderCharacter.SetSpeedModifier(modifier);
        foreach (var sprite in this.AllUnitSlots)
        {
            sprite.SetSpeedModifier(modifier);
        }
    }

    public void ToggleSpriteVisible(bool visible)
    {
        _leaderCharacter.ToggleSpriteVisible(visible);
        foreach (var sprite in this.AllUnitSlots)
        {
            sprite.ToggleSpriteVisible(visible);
        }
    }

    public void SetView(bool zoomedIn)
    {
        _zoomedView = zoomedIn;
        _leaderCharacter.ToggleSpriteVisible(!zoomedIn);

        if (!zoomedIn)
        {
            _leaderCharacter.SetIdle(_idle);
        }
        
        foreach (var sprite in this.AllUnitSlots)
        {
            sprite.ToggleSpriteVisible(zoomedIn);
            if (zoomedIn)
            {
                sprite.SetIdle(_idle);
            }
        }
    }

    public Transform GetObjectAtFormationPair(FormationPair pair)
    {
        var items = GetComponentsInChildren<ArmyMapViewUnit>(false);
        return items.FirstOrDefault(a => a.Matches(pair))?.transform;
    }

    public void SetSortingLayer(string layer, int value)
    {
        _leaderCharacter.SetSortingLayer(layer, value);
        foreach (var sprite in this.AllUnitSlots)
        {
            sprite.SetSortingLayer(layer, value);
        }
    }

    public void UpdateArmyOverlay()
    {
        if (this._army == null)
        {
            return;
        }

        // track morale and hp as well
        var morale = this._army.Morale;
        var hpPercent = this._army.GetHPPercent();
        var officer = this._army.GetOfficer();
        UnitStatSummary summary;
        if (officer == null)
        {
            var summaries = this._army.GetUnits(true).Select(a => a.GetStatSummary()).ToList();
            summary = UnitStatSummary.GetHighestAttAndDef(summaries);
        }
        else
        {
            summary = officer.GetStatSummary();
        }

        this._overlay.UpdateOverlay(summary, morale, hpPercent);
        foreach (var unit in this.ActiveUnits)
        {
            unit.UpdateArmyOverlay();
        }
    }
}
