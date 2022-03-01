using FlavBattle.Combat.Event;
using FlavBattle.Components;
using FlavBattle.Entities;
using FlavBattle.Entities.Data;
using FlavBattle.Entities.Modifiers;
using FlavBattle.Map;
using FlavBattle.Modifiers;
using FlavBattle.Pathfinding;
using FlavBattle.State;
using FlavBattle.Tilemap;
using FlavBattle.Trace;
using NaughtyAttributes;
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

public class Army : MonoBehaviour, ICombatArmy, IHasTraceData, ITrackableObject
{
    [Serializable]
    private class Detectors
    {
        [Required]
        [AllowNesting]
        public Detector AttackRangeDetector;

        [Required]
        [AllowNesting]
        public Detector ClickDetector;

        [Required]
        [AllowNesting]
        public Detector TileDetector;
    }

    public Army()
    {
        Formation = new Formation(this);
    }

    public float MoveStep = 1.0f;

    public event EventHandler<ArmyClickedEventArgs> ArmyClicked;

    public event EventHandler<ArmyEncounteredEventArgs> ArmyEncountered;

    public event EventHandler ArmyFledMap;

    public event EventHandler<EnterTileEventArgs> EnterTile;

    public event EventHandler<ExitTileEventArgs> ExitTile;

    public event EventHandler<ITrackableObject> Destroyed;

    public event Action Selected;

    public event Action Deselected;

    [SerializeField]
    private Detectors _detectors;

    private Vector3? _destination = null;
    private TravelPath _path = null;
    private TilemapManager _map = null;

    private bool _selected = false;
    public bool IsSelected => _selected;

    private GridTile _currentTile = null;
    private Vector3Int _currentTileCoords;
    private PathModifiers _cachedPathModifiers = null;

    [SerializeField]
    [Required]
    private ArmyModifierTracker _armyModifierTracker;

    public GridTile CurrentTileInfo => _currentTile;
    public bool IsFleeing { get; private set; } = false;

    /// <summary>
    /// Army is stuck preparing, such as after winning a battle.
    /// </summary>
    public bool IsPreparing { get; private set; } = false;

    /// <summary>
    /// How many seconds it takes for the preparation to finish
    /// </summary>
    public float PreparationTime { get; private set; } = 1.0f; 

    /// <summary>
    /// Whether the army is no longer part of the map, either due
    /// to having fled or having been destroyed in combat
    /// </summary>
    public bool IsDestroyed { get; private set; } = false;

    [Required]
    [SerializeField]
    private ArmyMapView _mapView;
    private ArmyMapView Animation => _mapView;

    [Tooltip("Events that happen in combat with this unit")]
    [SerializeField]
    private CombatConditionalEvent[] _combatEvents;
    public IEnumerable<CombatConditionalEvent> CombatEvents => _combatEvents;

    public SpriteRenderer FactionFlag;
    public SpriteRenderer FactionMarker;

    public FactionData Faction { get; private set; }
    public Formation Formation { get; private set; }

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

    /// <summary>
    /// Icon used to show special status like waiting or preparing.
    /// </summary>
    [Required]
    [SerializeField]
    private SpriteRenderer _spinningIcon;

    private ArmyTracker _tracker;

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
        _detectors.AttackRangeDetector.Detected += AttackRangeDetectorDetected;
        _detectors.TileDetector.Detected += TileDetectorEntered;
        _detectors.TileDetector.Exited += TileDetectorExited;
        _detectors.ClickDetector.Clicked += HandleDetectorClicked;

        _tracker = this.GetComponentInChildren<ArmyTracker>();
    }



    private void HandleDetectorClicked(object sender, MouseButton e)
    {
        ArmyClicked?.Invoke(this, new ArmyClickedEventArgs() { Clicked = this, Button = e });
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

    private void AttackRangeDetectorDetected(object sender, GameObject e)
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

    public void Select()
    {
        _selected = true;
        this.SetFootprints();
        Animation.SetColor(Color.cyan);
        Selected?.Invoke();
    }

    public void Unselect()
    {
        _selected = false;
        this._map.Footprints.Clear();
        Animation.SetColor(Color.white);
        Deselected?.Invoke();
    }

    public void CopyFrom(IArmy army)
    {
        ID = army.ID;
        Formation = army.Formation;
        SetFaction(army.Faction);
    }

    /// <summary>
    /// Gets the zoomed-in sprite representation for the unit in
    /// the formation pair location. Returns null if nothing there.
    /// </summary>
    public Transform GetSpriteAtFormationPair(FormationPair pair)
    {
        return _mapView.GetObjectAtFormationPair(pair);
    }

    /// <summary>
    /// Replaces the formation with the provided one.
    /// NOTE: Should ONLY be used for initialization.
    /// </summary>
    public void SetFormation(Formation formation)
    {
        this.Formation = formation;
        _mapView.SetArmy(this);
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
        yield return StartCoroutine(Animation.FadeAway());
        if (destroyOnVanish)
        {
            // Destroy(this.gameObject, 0.5f);
            DestroyArmy();
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

    public void SetPreparing(bool preparing)
    {
        // TODO: compute prep time
        IsPreparing = preparing;

        if (_spinningIcon == null)
        {
            return;
        }

        if (preparing)
        {
            _spinningIcon.sprite = GRM.CommonSprites.HourglassIcon;
            _spinningIcon.gameObject.Show();
        }
        else
        {
            _spinningIcon.gameObject.Hide();
        }

    }

    public void SetPaused(bool pause)
    {
        this._paused = pause;
    }

    public void ToggleZoomedView(bool zoomedIn)
    {
        this._mapView.SetView(zoomedIn);
    }

    public void StepTowardsDestination()
    {
        if (this._destination == null && this._path != null)
        {
            this.PlotNextPath();
        }

        if (this._destination != null)
        {
            var cost = _currentTile.GetTileCost(this.GetPathModifiers());
            if (cost <= 0.0f)
            {
                // Just a failsafe to avoid catastrophe
                Debug.LogError("Tile with cost 0 or less! Setting to 1");
                cost = 1.0f;
            }

            var modifier = MoveStep / cost;
            var delta = modifier * TimeUtils.AdjustedGameDelta;
            this.Animation.SetSpeedModifier(modifier);
            var newPos = Vector3.MoveTowards(this.transform.position, this._destination.Value, delta);
            this.transform.position = newPos;
            if ((newPos - this._destination.Value).magnitude <= delta)
            {
                this.transform.position = this._destination.Value;
                this._destination = null;
            }

            CheckCurrentLocation();
        }
    }

    public void SetDestination(Vector3? position)
    {
        this._destination = position;
    }

    /// <summary>
    /// Marks the army as destroyed and fires the Destroyed event,
    /// then setting the object as inactive. Basically deactivates
    /// the army without destroying the gameObject.
    /// </summary>
    [ContextMenu("Destroy Army")]
    public void DestroyArmy()
    {
        IsDestroyed = true;
        Destroyed?.Invoke(this, this);
        this.gameObject.SetActive(false);
    }

    public PathModifiers GetPathModifiers()
    {
        // TODO: this won't update when perks change
        if (_cachedPathModifiers == null)
        {
            _cachedPathModifiers = PathModifiers.CreateFromArmy(this);
        }

        return _cachedPathModifiers;
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
                this.Animation.SetIdle(true);
                this._map.Footprints.Clear();
                return;
            }
            else
            {
                var tile = this._path.Nodes.Dequeue();
                if (tile.Equals(_currentTile) && this._path.Nodes.Count > 0)
                {
                    // If we are already in the next tile and there are more tiles,
                    // move on to the next one in the path. This can happen when repathing.
                    tile = this._path.Nodes.Dequeue();
                }

                this.Animation.SetIdle(false);
                this._destination = new Vector3(tile.WorldX, tile.WorldY, 0);
                var facingLeft = tile.WorldX < this.transform.position.x;
                this.Animation.SetFlippedLeft(facingLeft);
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
            var furthest = MiscUtils.MathUtils.RandomFurthestPointAway(this.transform.position, points, 0.1f, 10);
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

    [ContextMenu("Set to fleeing")]
    public void SetToFleeing()
    {
        this.SetFleeing(true);
    }

    /// <summary>
    /// Checks if the unit of UnitData type is present in the
    /// formation.
    /// </summary>
    public bool HasUnitType(UnitData data)
    {
        if (data == null)
        {
            return false;
        }

        var units = this.GetUnits();
        return units.Any(a => a.SameType(data));
    }

    public TraceData GetTrace()
    {
        var unitTraces = this.GetUnits(false).Select(a => a.GetTrace());
        var data = TraceData.ChildTrace($"Army", unitTraces.ToArray());
        data.Context = this.gameObject;

        var tracker = this.GetComponentInChildren<ArmyTracker>();
        if (tracker != null)
        {
            data.Add(tracker.GetTrace());
        }

        data.Key = this.ID;
        return data;
    }

    public GameObject GetObject()
    {
        return this.gameObject;
    }

    public IEnumerable<IArmy> GetFlankingArmies()
    {
        if (_tracker != null)
        {
            return _tracker.GetFlankingArmies();
        }

        return new List<IArmy>();
    }

    public IEnumerable<IArmy> GetLinkedArmies()
    {
        
        if (_tracker != null)
        {
            return _tracker.GetLinkedArmies();
        }

        return new List<IArmy>();
    }

    public ModifierSet GetModifiers()
    {
        return this._armyModifierTracker.GetModifiers();
    }
}
