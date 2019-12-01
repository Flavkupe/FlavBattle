using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyClickedEventArgs : EventArgs
{
    public Army Clicked;
}

public class ArmyEncounteredEventArgs : EventArgs
{
    public Army Initiator;
    public Army Opponent;
}

public class Army : MonoBehaviour, IDetectable
{
    public float MoveStep = 1.0f;

    public event EventHandler<ArmyClickedEventArgs> ArmyClicked;

    public event EventHandler<ArmyEncounteredEventArgs> ArmyEncountered;

    private Vector3? _destination = null;
    private TravelPath _path = null;
    private TilemapManager _map = null;

    private AnimatedSprite _sprite;
    public SpriteRenderer FactionFlag;

    public FactionData Faction { get; private set; }
    public Formation Formation { get; } = new Formation();

    public DetectableType Type => DetectableType.Army;

    private Detector[] _detectors;
    private bool _paused = false;

    // Start is called before the first frame update
    void Start()
    {
        _sprite = this.GetComponentInChildren<AnimatedSprite>();
        _detectors = this.GetComponentsInChildren<Detector>();
        foreach (var detector in _detectors)
        {
            detector.Detected += Detector_Detected;
        }
    }

    private void Detector_Detected(object sender, GameObject e)
    {
        var other = e.GetComponent<Army>();
        if (other != null)
        {
            ArmyEncountered?.Invoke(this, new ArmyEncounteredEventArgs
            {
                Initiator = this,
                Opponent = other,
            });
        }
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (ArmyClicked != null)
            {
                ArmyClicked.Invoke(this, new ArmyClickedEventArgs() { Clicked = this });
            }
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

    // Update is called once per frame
    void Update()
    {
        if (_paused)
        {
            return;
        }

        if (this._destination != null)
        {
            this.MoveTowardsDestination();
        }
        else if (this._path != null)
        {
            this.PlotNextPath();
        }
    }

    public void SetMap(TilemapManager map)
    {
        this._map = map;
    }

    public void SetFaction(FactionData faction)
    {
        this.Faction = faction;
        this.FactionFlag.sprite = faction.Flag;
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

    public void SetPaused(bool pause)
    {
        this._paused = pause;
    }

    private void PlotNextPath()
    {
        if (this._path != null)
        {
            if (this._path.Nodes.Count == 0)
            {
                this._path = null;
                this._sprite.SetIdle(true);
                return;
            }
            else
            {
                var tile = this._path.Nodes.Dequeue();
                this._sprite.SetIdle(false);
                this._destination = new Vector3(tile.WorldX, tile.WorldY, 0);
            }
        }
    }


    private void MoveTowardsDestination()
    {
        if (this._destination != null)
        {
            var currentTile = this._map.GetGridTileAtWorldPos(this.transform.position.x, this.transform.position.y);
            var cost = Math.Max(1, currentTile.Data.WalkCost);
            var modifier = MoveStep / cost;
            var delta = modifier * Time.deltaTime;
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
