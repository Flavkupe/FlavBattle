using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NaughtyAttributes;

public class AnimatedMove : MonoBehaviour
{
    public Vector3[] Positions;

    [Tooltip("Seconds to wait between each move")]
    public float SecondsBetween = 1.0f;

    [Tooltip("Whether the animation affects localPosition rather than position")]
    public bool UseLocalPos = true;

    public float Speed = 5.0f;

    [SerializeField]
    private bool _allowKeyInterrupt;

    [Tooltip("Key used to interrupt moving animation")]
    [ShowIf("InteruptAllowed")]
    [SerializeField]
    private KeyCode _interruptKey;
    private bool _interrupted = false;
    private bool InteruptAllowed() => _allowKeyInterrupt;

    private bool ReachedDestination(Vector3 destination)
    {
        if (UseLocalPos)
        {
            return (Vector3.Distance(this.transform.localPosition, destination) < 0.1f);
        }
        else
        {
            return (Vector3.Distance(this.transform.position, destination) < 0.1f);
        }
    }
    
    private void SetPos(Vector3 pos)
    {
        if (UseLocalPos)
        {
            this.transform.localPosition = pos;
        }
        else
        {
            this.transform.position = pos;
        }
    }

    void Update()
    {
        if (InteruptAllowed() && Input.GetKey(_interruptKey))
        {
            _interrupted = true;
        }
    }

    public IEnumerator DoMove()
    {
        _interrupted = false;
        if (Positions.Length < 2)
        {
            Debug.LogWarning("Need at least 2 nodes for movement");
            yield break;
        }

        yield return DoMoveInternal();

        // Ensure it's at the end pos by end (even after interrupt)
        SetPos(Positions.Last());
        _interrupted = false;
    }

    private IEnumerator DoMoveInternal()
    {
        SetPos(Positions[0]);

        // Start with second element
        for (var i = 1; i < Positions.Length; i++)
        {
            var pos = Positions[i];
            while (!ReachedDestination(pos))
            {
                var tick = Speed * TimeUtils.AdjustedGameDeltaWithMouse;
                if (UseLocalPos)
                {
                    SetPos(Vector3.MoveTowards(this.transform.localPosition, pos, tick));
                }
                else
                {
                    SetPos(Vector3.MoveTowards(this.transform.position, pos, tick));
                }

                if (_interrupted)
                {
                    yield break;
                }

                yield return null;
            }

            if (i < Positions.Length - 1)
            {
                // Wait between steps unless it's the last step
                yield return new WaitForSecondsAccelerated(SecondsBetween, _interruptKey);
            }
        }
    }
}
