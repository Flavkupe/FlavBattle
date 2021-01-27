﻿using FlavBattle.State;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmyClickedEventArgs : EventArgs
{
    public Army Clicked;
    public MouseButton Button;
}

public class ArmyEncounteredEventArgs : EventArgs
{
    public Army Initiator;
    public Army Opponent;
}

public class EnterTileEventArgs : EventArgs
{
    public Army Initiator;
    public GameObject Tile;
}

public class ExitTileEventArgs : EventArgs
{
    public Army Initiator;
    public GameObject Tile;
}

public class Army : MonoBehaviour, IDetectable, IArmy
{
    public float MoveStep = 1.0f;

    public event EventHandler<ArmyClickedEventArgs> ArmyClicked;

    public event EventHandler<ArmyEncounteredEventArgs> ArmyEncountered;

    public event EventHandler ArmyFledMap;

    public event EventHandler<EnterTileEventArgs> EnterTile;

    public event EventHandler<ExitTileEventArgs> ExitTile;

    private Vector3? _destination = null;
    private TravelPath _path = null;
    private TilemapManager _map = null;
    
    private bool _selected = false;

    private GridTile _currentTile = null;
    private Vector3Int _currentTileCoords;

    public TileInfo CurrentTileInfo => _currentTile?.Info;
    public bool IsFleeing { get; private set; } = false;

    private AnimatedSprite _sprite;
    // Gets the AnimatedSprite for this, caching it if needed
    private AnimatedSprite AnimatedSprite
    {
        get
        {
            if (_sprite == null)
            {
                _sprite = this.GetComponentInChildren<AnimatedSprite>(true);
            }

            return _sprite;
        }
    }

    public SpriteRenderer FactionFlag;
    public SpriteRenderer FactionMarker;

    public FactionData Faction { get; private set; }
    public Formation Formation { get; private set; } = new Formation();

    public string ID { get; private set; }

    public bool IsPlayerArmy { get; private set; }

    public bool IsOnGarrison { get; private set; }

    /// <summary>
    /// Whether the Army is in motion somewhere
    /// </summary>
    public bool HasDestination => _destination != null || _path != null;

    public bool IsCommandable => IsPlayerArmy && !IsFleeing;

    public FightingStance Stance { get; set; }

    public Morale Morale { get; } = new Morale();

    public GameObject FlagIcon;

    public DetectableType Type => DetectableType.Army;

    private Detector[] _detectors;
    private bool _paused = false;



    void Awake()
    {
        if (ID == null)
        {
            ID = Guid.NewGuid().ToString();
        }
    }

    void Start()
    {
        _detectors = this.GetComponentsInChildren<Detector>();
        foreach (var detector in _detectors)
        {
            if (detector.Detects.HasFlag(DetectableType.Army))
            {
                detector.Detected += ArmyDetectorDetected;
            }

            if (detector.Detects.HasFlag(DetectableType.Tile))
            {
                detector.Detected += TileDetectorEntered;
                detector.Exited += TileDetectorExited;
            }
        }
    }

    private void TileDetectorExited(object sender, GameObject e)
    {
        var tile = e.GetComponent<IDetectable>();
        if (tile != null && tile.Type == DetectableType.Tile)
        {
            ExitTile?.Invoke(this, new ExitTileEventArgs()
            {
                Initiator = this,
                Tile = e
            });

            // Leaving garrison tile
            if (e.HasComponent<Garrison>())
            {
                this.IsOnGarrison = false;
            }

            if (e.HasComponent<Town>())
            {
                var town = e.GetComponent<Town>();
                town.Exited(this);
            }
        }
    }

    private void TileDetectorEntered(object sender, GameObject e)
    {
        var tile = e.GetComponent<IDetectable>();
        if (tile != null && tile.Type == DetectableType.Tile)
        {
            EnterTile?.Invoke(this, new EnterTileEventArgs()
            {
                Initiator = this,
                Tile = e
            });

            // Entering garrison tile
            if (e.HasComponent<Garrison>())
            {
                this.IsOnGarrison = true;
            }

            if (e.HasComponent<Town>())
            {
                var town = e.GetComponent<Town>();
                town.Entered(this);
            }
        }
    }

    private void ArmyDetectorDetected(object sender, GameObject e)
    {
        var otherArmy = e.GetComponent<Army>();
        if (otherArmy != null)
        {
            ArmyEncountered?.Invoke(this, new ArmyEncounteredEventArgs
            {
                Initiator = this,
                Opponent = otherArmy,
            });
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ArmyClicked?.Invoke(this, new ArmyClickedEventArgs() { Clicked = this, Button = MouseButton.LeftButton });
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ArmyClicked?.Invoke(this, new ArmyClickedEventArgs() { Clicked = this, Button = MouseButton.RightButton });
        }
    }

    public void Select()
    {
        _selected = true;
        this.SetFootprints();
        AnimatedSprite.SetColor(Color.cyan);
    }

    public void Unselect()
    {
        _selected = false;
        this._map.Footprints.Clear();
        AnimatedSprite.SetColor(Color.white);
    }

    public void CopyFrom(IArmy army)
    {
        ID = army.ID;
        Formation = army.Formation;
        SetFaction(army.Faction);
    }

    /// <summary>
    /// Replaces the formation with the provided one.
    /// NOTE: Should ONLY be used for initialization.
    /// </summary>
    public void SetFormation(Formation formation)
    {
        this.Formation = formation;

        var officer = formation.GetOfficer();
        if (officer != null)
        {
            this.AnimatedSprite.SetAnimations(officer.Data.Animations);
        }
    }

    /// <summary>
    /// Causes army to full flee from map (such as into exit point),
    /// vanishing and being destroyed.
    /// </summary>
    public void FleeMap()
    {
        ArmyFledMap?.Invoke(this, null);
        StartCoroutine(Vanish(true));
    }

    public IEnumerator Vanish(bool destroyOnVanish = false)
    {
        yield return StartCoroutine(AnimatedSprite.FadeAway());
        if (destroyOnVanish)
        {
            Destroy(this.gameObject, 0.5f);
        }
    }

    public void SetMap(TilemapManager map)
    {
        this._map = map;
        UpdateCurrentTile();
    }

    public void SetFaction(FactionData faction)
    {
        this.Faction = faction;
        this.FactionFlag.sprite = faction.Flag;
        IsPlayerArmy = faction.IsPlayerFaction;

        if (IsPlayerArmy)
        {
            this.FactionMarker.color = Color.blue;
        }
        else
        {
            this.FactionMarker.color = Color.red;
        }
    }

    public void PutOnTile(GridTile tile)
    {
        this.transform.position = new Vector3(tile.WorldX, tile.WorldY, 0);
    }

    public void SetPath(TravelPath path)
    {
        this._destination = null;
        this._path = path;
    }

    public void SetFleeing(bool fleeing)
    {
        IsFleeing = fleeing;
        if (fleeing)
        {
            FlagIcon.Show();
        }
        else
        {
            FlagIcon.Hide();
        }
    }

    public void SetPaused(bool pause)
    {
        this._paused = pause;
    }

    public void StepTowardsDestination()
    {
        if (this._destination == null && this._path != null)
        {
            this.PlotNextPath();
        }

        if (this._destination != null)
        {
            var cost = Math.Max(1, _currentTile.Info.WalkCost);
            var modifier = MoveStep / cost;
            var delta = modifier * TimeUtils.AdjustedGameDelta;
            this.AnimatedSprite.SetSpeedModifier(modifier);
            var newPos = Vector3.MoveTowards(this.transform.position, this._destination.Value, delta);
            this.transform.position = newPos;
            if ((newPos - this._destination.Value).magnitude <= delta)
            {
                this.transform.position = this._destination.Value;
                this._destination = null;
                this.AnimatedSprite.SetIdle(true);
            }

            CheckCurrentLocation();
        }


    }

    public void SetDestination(Vector3? position)
    {
        this._destination = position;
    }

    private void PlotNextPath()
    {
        if (this._path != null)
        {
            if (this._path.Nodes.Count == 0)
            {
                // Reached final destination
                AdjustForCollisions();
                this._path = null;
                this.AnimatedSprite.SetIdle(true);
                this._map.Footprints.Clear();
                return;
            }
            else
            {
                var tile = this._path.Nodes.Dequeue();
                this.AnimatedSprite.SetIdle(false);
                this._destination = new Vector3(tile.WorldX, tile.WorldY, 0);
                var facingLeft = tile.WorldX < this.transform.position.x;
                this.AnimatedSprite.SetFlipped(facingLeft);
                this.SetFootprints();
            }
        }
    }

    private void SetFootprints()
    {
        if (!_selected)
        {
            return;
        }

        this._map.Footprints.Clear();
        var nodeList = new List<Vector3>();
        if (_destination != null)
        {
            nodeList.Add(_destination.Value);
        }
        if (_path != null)
        {
            nodeList.AddRange(_path.Nodes.ToList().Select(a => a.ToWorldPos()));
            this._map.Footprints.CreatePath(nodeList);
        }
    }

    /// <summary>
    /// Uses a very rough Monte-Carlo heuristic to find a good spot to
    /// re-adjust if there are other units within range.
    /// </summary>
    private void AdjustForCollisions()
    {
        var collisions = Physics2D.OverlapCircleAll(this.transform.position, 0.1f)
                    .Where(a => a.gameObject != this.gameObject && a.gameObject.HasComponent<Army>()).ToList();

        if (collisions.Count > 0)
        {
            var points = collisions.Select(a => a.transform.position.ToVector2()).ToArray();
            var furthest = Utils.MathUtils.RandomFurthestPointAway(this.transform.position, points, 0.1f, 10);
            this.transform.position = furthest;
        }
    }

    /// <summary>
    /// Checks the current position on the map, as map coords. If
    /// the current position is a new grid coord from previous, updates
    /// to a new tile on the tilemap. This is a small optimization over
    /// always updating the tilemap on every frame.
    /// </summary>
    private void CheckCurrentLocation()
    {
        // After moving, check if this is a new tile
        var x = this.transform.position.x;
        var y = this.transform.position.y;
        var currentTileCoords = this._map.GetGridCoordsAtWorldPos(x, y);
        if (!currentTileCoords.Equals(_currentTileCoords))
        {
            // on a new grid coord    
            UpdateCurrentTile();
        }
    }

    /// <summary>
    /// Updates the current tile to match the 
    /// </summary>
    private void UpdateCurrentTile()
    {
        var x = this.transform.position.x;
        var y = this.transform.position.y;
        _currentTileCoords = this._map.GetGridCoordsAtWorldPos(x, y);
        _currentTile = this._map.GetGridTileAtWorldPos(x, y);
    }

    public GameObject GetObject()
    {
        return this.gameObject;
    }

    [ContextMenu("Set to fleeing")]
    public void SetToFleeing()
    {
        this.SetFleeing(true);
    }
}
