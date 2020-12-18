using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialTimer : MonoBehaviour
{
    [Required]
    public SpriteMask Mask;

    /// <summary>
    /// Sets the percent (0.0f to 1.0f, inclusive)
    /// </summary>
    /// <param name="percent"></param>
    public void SetPercentage(float percent)
    {
        percent = Mathf.Min(1.0f, percent);

        // Take the inverse; 100% is closer to 0.
        percent = 1 - percent;

        // If it's 0.0f, then the entire mask is ignored; don't want that
        percent = Mathf.Max(0.001f, percent);

        Mask.alphaCutoff = percent;
    }
}
