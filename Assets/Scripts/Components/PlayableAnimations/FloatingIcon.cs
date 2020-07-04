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

    public override void PlayAnimation()
    {
        StartCoroutine(PlayToCompletion());
    }

    public override IEnumerator PlayToCompletion()
    {
        var renderer = this.gameObject.GetComponent<SpriteRenderer>();
        renderer.sprite = Sprite;
        this.transform.position += Offset;

        while (Duration > 0.0f)
        {
            Duration -= Time.deltaTime;
            this.transform.position = this.transform.position.ShiftY(Speed * Time.deltaTime);
            yield return null;
        }

        renderer.enabled = false;
        Destroy(this.gameObject, 1.0f);
    }
}
