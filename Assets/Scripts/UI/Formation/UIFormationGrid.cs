using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFormationGrid : MonoBehaviour, IFormationGrid
{
    public UIFormationGridTile TileTemplate;

    public List<UIFormationGridTile> Slots { get; } = new List<UIFormationGridTile>();

    public IFormationGridSlot CreateSlot()
    {
        var slot = Instantiate(TileTemplate);
        Slots.Add(slot);
        return slot;
    }
}
