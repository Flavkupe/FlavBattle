﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyClickedEventArgs : EventArgs
{
    public Army Clicked;
}

public class Army : MonoBehaviour
{
    public Formation _formation = new Formation();

    public float MoveStep = 1.0f;

    public event EventHandler<ArmyClickedEventArgs> ArmyClicked;

    private Vector3? _destination = null;
    private TravelPath _path = null;
    private TilemapManager _map = null;

    private bool _clicked = false;
    private bool _selected = false;

    private AnimatedSprite _sprite;

    public Formation Formation => _formation;

    // Start is called before the first frame update
    void Start()
    {
        _sprite = this.GetComponentInChildren<AnimatedSprite>();
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
        if (this._destination != null)
        {
            this.MoveTowardsDestination();
        }
        else if (this._path != null)
        {
            this.PlotNextPath();
        }
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

    public void SetMap(TilemapManager map)
    {
        this._map = map;
    }

    public void PutOnTile(GridTile tile)
    {
        this.transform.position = new Vector3(tile.WorldX, tile.WorldY, 0);
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

    public void SetPath(TravelPath path)
    {
        this._destination = null;
        this._path = path;
    }

    private void SetDestination(Vector3? position)
    {
        this._destination = position;
    }
}
