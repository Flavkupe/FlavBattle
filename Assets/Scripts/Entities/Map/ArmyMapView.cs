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

    [Required]
    [SerializeField]
    private Vector3 _animatedCharacterOffset = new Vector3(0.0f, -0.15f, 0.0f);

    [Required]
    [SerializeField]
    private Vector3 _animatedCharacterScale = new Vector3(0.5f, 0.5f, 1.0f);

    [Required]
    [SerializeField]
    private AnimatedSprite _mainSprite;

    private IAnimatedSprite _main;

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
        if (mainUnit.Data.AnimatedCharacter != null)
        {
            _mainSprite.ToggleSpriteVisible(false);
            var character = Instantiate(mainUnit.Data.AnimatedCharacter, this.transform, false);
            character.transform.localPosition = _animatedCharacterOffset;
            character.transform.localScale = _animatedCharacterScale;
            _main = character;
        }
        else
        {
            // TODO: remove
            _mainSprite.ToggleSpriteVisible(true);
            _mainSprite.SetAnimations(mainUnit.Data.Animations);
            _main = _mainSprite;
        }

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
            if (unit.Data.AnimatedCharacter != null)
            {
                sprite.ToggleSpriteVisible(false);
                var instance = Instantiate(unit.Data.AnimatedCharacter, slot.transform, false);
                instance.transform.localPosition = Vector3.zero;
                this._animatedSprites.Add(instance);
            }
            else
            {
                // TODO: remove once we have all animated chars
                sprite.ToggleSpriteVisible(true);
                sprite.SetAnimations(unit.Data.Animations);
                this._animatedSprites.Add(sprite);
            }
            
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
        _main.SetFlippedLeft(flipped);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetFlippedLeft(flipped);
        }
    }

    public void SetIdle(bool idle)
    {
        _main.SetIdle(idle);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetIdle(idle);
        }
    }

    public void SetColor(Color color)
    {
        _main.SetColor(color);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetColor(color);
        }
    }

    public void SetSpeedModifier(float modifier)
    {
        _main.SetSpeedModifier(modifier);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetSpeedModifier(modifier);
        }
    }

    public void ToggleSpriteVisible(bool visible)
    {
        _main.ToggleSpriteVisible(visible);
        foreach (var sprite in this._animatedSprites)
        {
            sprite.ToggleSpriteVisible(visible);
        }
    }

    public void SetView(bool zoomedIn)
    {
        _main.ToggleSpriteVisible(!zoomedIn);
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
}
