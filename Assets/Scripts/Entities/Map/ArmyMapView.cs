using FlavBattle.Core;
using FlavBattle.Formation;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using FlavBattle.Components;

public class ArmyMapView : MonoBehaviour, IAnimatedSprite
{
    private Dictionary<FormationPair, WithFormation> _units = new Dictionary<FormationPair, WithFormation>();

    private List<IAnimatedSprite> _animatedSprites = new List<IAnimatedSprite>();

    [SerializeField]
    private AnimatedCharacterVisuals _characterVisualProperties;

    private IAnimatedSprite _leaderCharacter;

    private CameraMain _cam;

    private IArmy _army;

    // Start is called before the first frame update
    void Start()
    {
        _cam = FindObjectOfType<CameraMain>(true);

        if (_units.Count == 0)
        {
            Init();
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

        var mainUnit = army.Formation.GetOfficer(true);
        var character = Instantiate(mainUnit.Data.AnimatedCharacter, this.transform, false);
        character.SetVisuals(this._characterVisualProperties);
        _leaderCharacter = character;

        SetFormation(army.Formation);

        var zoomedIn = _cam?.IsZoomedDistance() ?? false;
        SetView(zoomedIn);
    }

    private void SetFormation(Formation formation)
    {
        // First hide each so only the ones in the formation
        // are later shown
        foreach (var item in _units)
        {
            item.Value.Hide();
        }

        foreach (var unit in formation.GetUnits())
        {
            var slot = _units[unit.Formation];
            slot.Show();
            var instance = Instantiate(unit.Data.AnimatedCharacter, slot.transform, false);
            instance.transform.localPosition = Vector3.zero;
            this._animatedSprites.Add(instance);
        }
    }

    private void Clear()
    {
        this._animatedSprites.Clear();
        var animatedCharacters = this.GetComponentsInChildren<AnimatedCharacter>();
        foreach (var item in animatedCharacters)
        {
            Destroy(item.gameObject);
        }
    }

    private void HandleFormationChanged(object obj, Formation e)
    {
        SetFormation(e);   
    }

    private void Init()
    {
        Clear();
        foreach (var item in GetComponentsInChildren<WithFormation>(true))
        {
            var pair = item.Pair;
            _units[pair] = item;
        }
    }

    public void SetFlippedLeft(bool flipped)
    {
        _leaderCharacter.SetFlippedLeft(flipped);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetFlippedLeft(flipped);
        }
    }

    public void SetIdle(bool idle)
    {
        _leaderCharacter.SetIdle(idle);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetIdle(idle);
        }
    }

    public void SetColor(Color color)
    {
        _leaderCharacter.SetColor(color);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetColor(color);
        }
    }

    public void SetSpeedModifier(float modifier)
    {
        _leaderCharacter.SetSpeedModifier(modifier);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetSpeedModifier(modifier);
        }
    }

    public void ToggleSpriteVisible(bool visible)
    {
        _leaderCharacter.ToggleSpriteVisible(visible);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.ToggleSpriteVisible(visible);
        }
    }

    public void SetView(bool zoomedIn)
    {
        _leaderCharacter.ToggleSpriteVisible(!zoomedIn);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.ToggleSpriteVisible(zoomedIn);
        }
    }

    public Transform GetObjectAtFormationPair(FormationPair pair)
    {
        var items = GetComponentsInChildren<WithFormation>(false);
        return items.FirstOrDefault(a => a.Matches(pair))?.transform;
    }

    public void SetSortingLayer(string layer, int value)
    {
        _leaderCharacter.SetSortingLayer(layer, value);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetSortingLayer(layer, value);
        }
    }
}
