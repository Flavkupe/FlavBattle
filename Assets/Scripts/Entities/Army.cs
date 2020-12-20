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
    public static Army CreateFromFormation(Army template, Formation formation)
    {
        var army = Instantiate(template);
        army.Formation = formation;
        return army;
    }

    public float MoveStep = 1.0f;

    public event EventHandler<ArmyClickedEventArgs> ArmyClicked;

    public event EventHandler<ArmyEncounteredEventArgs> ArmyEncountered;

    public event EventHandler<EnterTileEventArgs> EnterTile;

    public event EventHandler<ExitTileEventArgs> ExitTile;

    private Vector3? _destination = null;
    private TravelPath _path = null;
    private TilemapManager _map = null;
    private bool _fleeing = false;

    private AnimatedSprite _sprite;
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
    public bool HasDestination => _destination != null;

    public bool IsCommandable => IsPlayerArmy && !_fleeing;

    public FightingStance Stance { get; set; }

    public Morale Morale { get; } = new Morale();

    public GameObject FlagIcon;

    public DetectableType Type => DetectableType.Army;

    private Detector[] _detectors;
    private bool _paused = false;

    private List<ArmyMapAIBase> _aiActions = new List<ArmyMapAIBase>();
    private ArmyMapAIBase _currentAction = null;

    void Awake()
    {
        if (ID == null)
        {
            ID = Guid.NewGuid().ToString();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _sprite = this.GetComponentInChildren<AnimatedSprite>();
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

        _aiActions.AddRange(GetComponents<ArmyMapAIBase>());
        _aiActions = _aiActions.OrderBy(a => a.Priority).ToList();
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
        _sprite.SetColor(Color.cyan);
    }

    public void Unselect()
    {
        _sprite.SetColor(Color.white);
    }

    public void CopyFrom(IArmy army)
    {
        ID = army.ID;
        Formation = army.Formation;
        SetFaction(army.Faction);
    }

    // Update is called once per frame
    void Update()
    {
        if (_paused || GameState.IsMapPaused)
        {
            return;
        }

        // TODO; some events should cause all armies to
        // have to revisit plans
        if (this._destination != null)
        {
            this.StepTowardsDestination();
        }
        else if (this._path != null)
        {
            this.PlotNextPath();
        }
        else
        {
            this.PlotNextAction();
        }
    }

    public Coroutine Vanish()
    {
        return StartCoroutine(VanishInternal());
    }

    public void SetMap(TilemapManager map)
    {
        this._map = map;
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
        _fleeing = fleeing;
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

    private IEnumerator VanishInternal()
    {
        yield return _sprite.FadeAway();
    }

    // TODO: there should be a cooldown for this
    private void PlotNextAction()
    {
        if (_aiActions.Count == 0)
        {
            return;
        }

        if (_currentAction != null && _currentAction.ShouldContinueAction)
        {
            _currentAction.DoActionTick(this, _map);
            return;
        }

        foreach (var action in _aiActions)
        {
            if (action.IsActionPossible(this, _map))
            {
                _currentAction = action;
                action.DoActionTick(this, _map);
                return;
            }
        }
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
                this._sprite.SetIdle(true);
                this.SetFleeing(false);
                return;
            }
            else
            {
                var tile = this._path.Nodes.Dequeue();
                this._sprite.SetIdle(false);
                this._destination = new Vector3(tile.WorldX, tile.WorldY, 0);
                var facingLeft = tile.WorldX < this.transform.position.x;
                this._sprite.SetFlipped(facingLeft);
            }
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

    private void StepTowardsDestination()
    {
        if (this._destination != null)
        {
            var currentTile = this._map.GetGridTileAtWorldPos(this.transform.position.x, this.transform.position.y);
            var cost = Math.Max(1, currentTile.Data.WalkCost);
            var modifier = MoveStep / cost;
            var delta = modifier * TimeUtils.AdjustedGameDelta;
            this._sprite.SetSpeedModifier(modifier);
            var newPos = Vector3.MoveTowards(this.transform.position, this._destination.Value, delta);
            this.transform.position = newPos;
            if ((newPos - this._destination.Value).magnitude <= delta)
            {
                this.transform.position = this._destination.Value;
                this._destination = null;
                this._sprite.SetIdle(true);
            }
        }
    }

    private void SetDestination(Vector3? position)
    {
        this._destination = position;
    }

    public GameObject GetObject()
    {
        return this.gameObject;
    }
}
