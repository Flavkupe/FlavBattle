using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfficerCommandsPanel : MonoBehaviour
{
    public IconLabelPair IconLabelPairModel;
    public GameObject Panel;

    public void SetUnit(Unit unit)
    {
        Clear();
        if (!unit.IsOfficer)
        {
            return;
        }

        foreach (var ability in unit.Info.OfficerAbilities)
        {
            var pair = Instantiate(IconLabelPairModel);
            pair.SetIcon(ability.Icon);
            pair.SetText(ability.Name);

            pair.transform.SetParent(Panel.transform);

            // TODO: description tooltip
        }
    }

    public void Clear()
    {
        foreach (var child in Panel.transform.GetComponentsInChildren<IconLabelPair>())
        {
            Destroy(child.gameObject);
        }
    }
}
