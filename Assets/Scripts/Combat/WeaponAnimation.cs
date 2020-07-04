using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum WeaponRotation
{
    /// <summary>
    /// Graphic is facing straight up; requires no rotation
    /// </summary>
    StraightUp,

    /// <summary>
    /// Graphic is at an angle, with the top to the left
    /// </summary>
    UpLeft,

    /// <summary>
    /// Graphic is at an angle, with the top to the right
    /// </summary>
    UpRight
}

/// <summary>
/// Plays an attached animation which involves animating a single weapon Sprite.
/// Allows flexibility to replace the weapon using ReplaceWeapon.
/// Must specify the direction the weapon sprite is facing so it may be rotated;
/// all animations assume the sprite is pointing straight up.
/// </summary>
[RequireComponent(typeof(Animation))]
public class WeaponAnimation : CombatAnimation
{
    [Tooltip("A weapon to use for the animation, which will replace the custom one from the animation. Make sure it's pointing straight up.")]
    [AssetIcon]
    [Required]
    public Sprite Weapon;

    [Tooltip("The renderer that is used by the animation, which will be replaced with the specified weapon")]
    [Required]
    public SpriteRenderer AnimatedRenderer;

    [Tooltip("How the weapon graphic is oriented. Weapon will be rotated to be upright.")]
    public WeaponRotation Rotation;

    protected override void Awake()
    {
        base.Awake();
        ReplaceWeapon(Weapon, Rotation);
    }

    /// <summary>
    /// Replaces the weapon for the animation with a new sprite.
    /// Must specify orientation of sprite so it can be rotated
    /// </summary>
    /// <param name="weapon">The sprite to replace the weapon with.</param>
    /// <param name="rotation">Orientation of the weapon in the sprite (which way is it facing?)</param>
    public void ReplaceWeapon(Sprite weapon, WeaponRotation rotation)
    {
        Weapon = weapon;
        AnimatedRenderer.sprite = weapon;

        switch (rotation)
        {
            case WeaponRotation.UpLeft:
                AnimatedRenderer.gameObject.transform.Rotate(0, 0, -45);
                break;
            case WeaponRotation.UpRight:
                AnimatedRenderer.gameObject.transform.Rotate(0, 0, 45);
                break;
        }
    }
}
