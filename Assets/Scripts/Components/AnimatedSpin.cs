using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedSpin : MonoBehaviour
{
    public float Speed = 600.0f;

    public int Times = 3;

    public Vector3 Axis;

    public float SlowdownBase = 3.0f;

    public bool FadeAndDestroyOnComplete;

    [Tooltip("Which sort of acceleration is applied to the spin (mouse, gamespeed, etc). Defaults to all.")]
    public AccelOption Acceleration = AccelOption.MouseAndGameSpeed;

    public float FadeDelay = 2.0f;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    public Coroutine SpinAround()
    {
        return SpinAround(Axis, Times, Speed);
    }

    public Coroutine SpinAround(Vector3 axis, int times, float speed)
    {
        return StartCoroutine(SpinAroundInternal(axis, times, speed));   
    }

    private IEnumerator SpinAroundInternal(Vector3 axis, int times, float speed)
    {
        var angle = 0.0f;
        while (times > 0)
        {
            var delta = TimeUtils.AdjustedDelta(Acceleration);
            var rate = speed * delta;
            angle += rate;
            this.transform.Rotate(axis, rate);
            if (angle > 360.0f)
            {
                angle = 0.0f;
                times--;
            }

            yield return null;
        }

        angle = 0.0f;
        var baseSpeed = speed;
        while (angle < 360.0f)
        {
            var delta = TimeUtils.AdjustedDelta(Acceleration);
            speed = Mathf.Max(baseSpeed / SlowdownBase, speed * 0.9f);
            var rate = speed * delta;
            angle += rate;
            this.transform.Rotate(axis, rate);
            yield return null;
        }

        if (this.FadeAndDestroyOnComplete)
        {
            yield return FadeAndDestroy();
        }
    }

    private IEnumerator FadeAndDestroy()
    {
        var renderers = this.GetComponentsInChildren<SpriteRenderer>();
        var timer = FadeDelay;
        while (timer > 0.0f)
        {
            timer -= TimeUtils.AdjustedDelta(Acceleration);
            yield return null;
        }

        var alpha = 1.0f;
        while (alpha > 0.0f)
        {
            alpha -= TimeUtils.AdjustedDelta(Acceleration);
            foreach (var renderer in renderers)
            {
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, alpha);
            }

            yield return null;
        }

        Destroy(this.gameObject);
    }
}
