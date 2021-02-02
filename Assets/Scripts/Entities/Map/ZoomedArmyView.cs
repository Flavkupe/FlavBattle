using FlavBattle.Formation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomedArmyView : MonoBehaviour, IAnimatedSprite
{
    private Dictionary<FormationPair, WithFormation> _units = new Dictionary<FormationPair, WithFormation>();

    private List<IAnimatedSprite> _animatedSprites = new List<IAnimatedSprite>();

    // Start is called before the first frame update
    void Start()
    {
        if (_units.Count == 0)
        {
            Init();
        }
    }

    public void Clear()
    {
        foreach (var item in _units)
        {
            item.Value.Hide();
        }
    }

    public void SetArmy(IArmy army)
    {
        if (_units.Count == 0)
        {
            Init();
        }

        Clear();
        foreach (var unit in army.GetUnits())
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

    private void Init()
    {
        this._animatedSprites.Clear();
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
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetFlipped(flipped);
        }
    }

    public void SetIdle(bool idle)
    {
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetIdle(idle);
        }
    }

    public void SetAnimations(Sprite[] animations)
    {
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetAnimations(animations);
        }
    }

    public void SetColor(Color color)
    {
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetColor(color);
        }
    }

    public void SetSpeedModifier(float modifier)
    {
        foreach (var sprite in this._animatedSprites)
        {
            sprite.SetSpeedModifier(modifier);
        }
    }
}
