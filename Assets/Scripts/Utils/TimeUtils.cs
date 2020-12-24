using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum GameSpeed
{
    /// <summary>
    /// Zero speed
    /// </summary>
    Pause,

    /// <summary>
    /// Half speed
    /// </summary>
    Slow,

    /// <summary>
    /// Normal speed (multiplier of 1)
    /// </summary>
    Normal,

    /// <summary>
    /// 1.5x the speed
    /// </summary>
    Fast,

    /// <summary>
    /// Double the speed
    /// </summary>
    VeryFast,

    /// <summary>
    /// 4x the speed
    /// </summary>
    UltraFast,
}

public enum AccelOption
{
    /// <summary>
    /// No acceleration multiplier. Uses standard Time.deltaTime.
    /// </summary>
    None,

    /// <summary>
    /// Whether the acceleration is only affected by game speed (no mouse input)
    /// </summary>
    GameSpeedOnly,

    /// <summary>
    /// Whether the acceleration is only affected by mouse being down (no
    /// effect from gamespeed)
    /// </summary>
    MouseAccelOnly,

    /// <summary>
    /// Whether both gamespeed and mouse being down affect speed multiplier
    /// </summary>
    MouseAndGameSpeed,
}

/// <summary>
/// Speed configuration for any component of the game.
/// </summary>
public class GameSpeedConfig
{
    private GameSpeed _speed = GameSpeed.Normal;

    private GameSpeed _prepauseSpeed = GameSpeed.Normal;

    public GameSpeed GetGameSpeed()
    {
        return _speed;
    }

    public void SetGameSpeed(GameSpeed speed)
    {
        _speed = speed;
    }

    /// <summary>
    /// Remember previous speed and pause. Unpausing
    /// will revert to previous speed. If game already
    /// Paused, will do nothing.
    ///
    /// </summary>
    public void Pause()
    {
        if (_speed != GameSpeed.Pause)
        {
            _prepauseSpeed = _speed;
            _speed = GameSpeed.Pause;
        }
    }

    /// <summary>
    /// If paused, will call Unpause(). Otherwise calls Pause().
    /// </summary>
    public void TogglePause()
    {
        if (_speed != GameSpeed.Pause)
        {
            Pause();
        }
        else
        {
            Unpause();
        }
    }

    /// <summary>
    /// If paused, reverts speed to pre-pause speed.
    /// Does nothing if not paused.
    /// </summary>
    public void Unpause()
    {
        if (_speed == GameSpeed.Pause)
        {
            _speed = _prepauseSpeed;
        }
    }

    /// <summary>
    /// Gets the Time.deltaTime adjusted by a multiplier affected by the provided
    /// options combined with game settings.
    /// </summary>
    /// <param name="options">Which applicable multipliers to use.</param>
    public float GetAdjustedDeltaTime(AccelOption options = AccelOption.GameSpeedOnly)
    {
        var multiplier = 1.0f;

        if (options == AccelOption.GameSpeedOnly|| options == AccelOption.MouseAndGameSpeed)
        {
            // If gamespeed accel specified, use gamespeed multiplier
            multiplier = GetSpeedMultiplier();
        }
            
        if ((options == AccelOption.MouseAccelOnly || options == AccelOption.MouseAndGameSpeed)
            && Input.GetMouseButton(0))
        {
            // Greatly speedup if mouse button down, if mouseAccelerate enabled
            multiplier *= 2.0f;
        }

        return multiplier * Time.deltaTime;
    }

    private float GetSpeedMultiplier()
    {
        switch (_speed)
        {
            case GameSpeed.Pause:
                return 0.0f;
            case GameSpeed.Slow:
                return 0.5f;
            case GameSpeed.Fast:
                return 1.5f;
            case GameSpeed.VeryFast:
                return 2.0f;
            case GameSpeed.UltraFast:
                return 4.0f;
            default:
            case GameSpeed.Normal:
                return 1.0f;
        }
    }
}

/// <summary>
/// Utils dealing with the passing of gametime
/// </summary>
public static class TimeUtils
{
    /// <summary>
    /// General speed configurations for the game.
    /// </summary>
    public static GameSpeedConfig GameSpeed { get; } = new GameSpeedConfig();

    public static float AdjustedDelta(AccelOption option) => GameSpeed.GetAdjustedDeltaTime(option);

    /// <summary>
    /// Shortcut to TimeUtils.GameSpeed.GetAdjustedDeltaTime(AccelOption.GameSpeedOnly)
    /// 
    /// This is affected by GameSpeed but not mousedown.
    /// 
    /// Use this for things affected by game speed that should not speed up
    /// when mouse is down, like overworld army walking animations.
    /// </summary>
    public static float AdjustedGameDelta => GameSpeed.GetAdjustedDeltaTime();

    /// <summary>
    /// Shortcut to TimeUtils.GameSpeed.GetAdjustedDeltaTime(AccelOption.MouseAccelOnly)
    /// 
    /// This is affected by mousedown but not GameSpeed.
    /// 
    /// Use this for animations that should be sped up but not affected by gamespeed
    /// (cinematics, etc)
    /// </summary>
    public static float AdjustedGameDeltaWithMouse => GameSpeed.GetAdjustedDeltaTime(AccelOption.MouseAccelOnly);

    /// <summary>
    /// Shortcut to TimeUtils.GameSpeed.GetAdjustedDeltaTime(AccelOption.MouseAndGameSpeed)
    /// 
    /// This is affected by both mousedown and GameSpeed.
    /// 
    /// Used by things like animations that should both be affected by gamespeed and mousedown, such as
    /// combat animations.
    /// </summary>
    public static float FullAdjustedGameDelta => GameSpeed.GetAdjustedDeltaTime(AccelOption.MouseAndGameSpeed);
}

/// <summary>
/// Like WaitForSeconds, but accelerated by the left mouseclick
/// </summary>
public class WaitForSecondsAccelerated : CustomYieldInstruction
{
    private float _seconds;

    private KeyCode? _interruptKey;

    public WaitForSecondsAccelerated(float seconds)
    {
        _seconds = seconds;
    }

    public WaitForSecondsAccelerated(float seconds, KeyCode interruptKey)
    {
        _seconds = seconds;
        _interruptKey = interruptKey;
    }

    public override bool keepWaiting
    {
        get
        {
            if (_interruptKey.HasValue && Input.GetKey(_interruptKey.Value))
            {
                return false;
            }

            _seconds -= TimeUtils.AdjustedGameDeltaWithMouse;
            return _seconds > 0.0f;
        }
    }
}

/// <summary>
/// A simple cooldown timer based on ticks. Will add delta to the total, and
/// when the total reaches a fixed amount, will return true.
/// 
/// Use it to throttle behaviors that otherwise happen too often, like checking
/// for win conditions.
/// </summary>
public class ThrottleTimer
{
    private float _ticks = 1.0f;
    private float _current = 0.0f;

    public ThrottleTimer(float ticks = 1.0f)
    {
        _ticks = ticks;
    }

    /// <summary>
    /// Adds delta to the timer. If the timer reaches the
    /// threshold, reset the cooldown and return true. Otherwise,
    /// return false.
    /// </summary>
    public bool Tick(float delta)
    {
        _current += delta;
        if (_current >= _ticks)
        {
            _current = 0.0f;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Ticks using Time.deltaTime.
    /// </summary>
    public bool Tick()
    {
        return Tick(Time.deltaTime);
    }
}