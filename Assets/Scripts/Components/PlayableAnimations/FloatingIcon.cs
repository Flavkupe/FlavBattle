using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(SpriteRenderer))]
public class FloatingIcon : PlayableAnimation
{
    [Required]
    [ShowAssetPreview]
    [AssetIcon]
    public Sprite Sprite;

    [Tooltip("Duration in seconds")]
    public float Duration = 1.0f;

    [Tooltip("Starting offset when this is created")]
    public Vector3 Offset;

    [Tooltip("Whether this should float up over time as the animation")]
    public bool FloatUp = true;

    [Tooltip("Whether this should float up over time as the animation")]
    public bool ChangeScale = false;

    [Tooltip("Initial scale, in case of ChangeScale being true")]
    [ShowIf("ChangeScale")]
    public Vector3 ScaleStart;

    [Tooltip("Initial scale, in case of ChangeScale being true")]
    [ShowIf("ChangeScale")]
    public Vector3 ScaleEnd;

    public override void PlayAnimation()
    {
        StartCoroutine(PlayToCompletion());
    }

    public override IEnumerator PlayToCompletion()
    {
        var renderer = this.gameObject.GetComponent<SpriteRenderer>();
        renderer.sprite = Sprite;
        this.transform.position += Offset;
        if (ChangeScale)
        {
            this.transform.localScale = ScaleStart;
        }

        while (Duration > 0.0f)
        {
            var delta = TimeUtils.FullAdjustedGameDelta;
            Duration -= delta;

            if (FloatUp)
            {
                this.transform.position = this.transform.position.ShiftY(Speed * delta);
            }

            if (ChangeScale)
            {
                this.transform.localScale = Vector3.Lerp(this.transform.localScale, ScaleEnd, Speed * delta);
            }

            yield return null;
        }

        renderer.enabled = false;
        Destroy(this.gameObject, 1.0f);
    }
}
