
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// A component that has a reference to a tooltip object to be used as a
/// tooltip in any child objects to this one. This component is used
/// for searching for the proper tooltip object in case there are multiple
/// tooltips in the same Scene.
/// </summary>
public class TooltipHolder : MonoBehaviour
{
    [Required]
    public Tooltip Tooltip;
}
