using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army : MonoBehaviour
{
    private Vector3? destination = null;

    private TravelPath path = null;

    public float MoveStep = 1.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (this.destination != null)
        {
            this.MoveTowardsDestination();
        }
        else if (this.path != null)
        {
            this.PlotNextPath();
        }
    }

    private void PlotNextPath()
    {
        if (this.path != null)
        {
            if (this.path.Nodes.Count == 0)
            {
                this.path = null;
                return;
            }
            else
            {
                var next = this.path.Nodes.Dequeue();
                this.destination = new Vector3(next.Tile.WorldX, next.Tile.WorldY, 0);
            }
        }
    }

    private void MoveTowardsDestination()
    {
        if (this.destination != null)
        {
            var delta = MoveStep * Time.deltaTime;
            var newPos = Vector3.MoveTowards(this.transform.position, this.destination.Value, delta);
            this.transform.position = newPos;
            if ((newPos - this.destination.Value).magnitude <= delta)
            {
                this.transform.position = this.destination.Value;
                this.destination = null;
            }
        }
    }

    public void SetPath(TravelPath path)
    {
        this.destination = null;
        this.path = path;
    }

    public void SetDestination(Vector3? position)
    {
        this.destination = position;
    }
}
