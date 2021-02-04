using FlavBattle.Core;
using FlavBattle.Formation;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyMapView : MonoBehaviour, IAnimatedSprite
{
    private Dictionary<FormationPair, WithFormation> _units = new Dictionary<FormationPair, WithFormation>();

    private List<IAnimatedSprite> _animatedSprites = new List<IAnimatedSprite>();

    [Required]
    [SerializeField]
    private AnimatedSprite _mainSprite;

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
        _mainSprite.SetAnimations(mainUnit.Data.Animations);


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
            var sprite = slot.GetComponent<AnimatedSprite>();
            if (sprite != null)
            {
                sprite.SetAnimations(unit.Data.Animations);
            }
        }
    }

    private void Clear()
    {
        this._animatedSprites.Clear();
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
            var sprite = item.GetComponent<AnimatedSprite>();
            if (sprite != null)
            {
                this._animatedSprites.Add(sprite);
            }
        }
    }

    public void SetFlipped(bool flipped)
    {
        _mainSprite.SetFlipped(flipped);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetFlipped(flipped);
        }
    }

    public void SetIdle(bool idle)
    {
        _mainSprite.SetIdle(idle);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetIdle(idle);
        }
    }

    public void SetAnimations(Sprite[] animations)
    {
        _mainSprite.SetAnimations(animations);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetAnimations(animations);
        }
    }

    public void SetColor(Color color)
    {
        _mainSprite.SetColor(color);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetColor(color);
        }
    }

    public void SetSpeedModifier(float modifier)
    {
        _mainSprite.SetSpeedModifier(modifier);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetSpeedModifier(modifier);
        }
    }

    public void ToggleSpriteVisible(bool visible)
    {
        _mainSprite.ToggleSpriteVisible(visible);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.ToggleSpriteVisible(visible);
        }
    }

    public void SetView(bool zoomedIn)
    {
        _mainSprite.ToggleSpriteVisible(!zoomedIn);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.ToggleSpriteVisible(zoomedIn);
        }
    }
}
