using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AttackStats : MonoBehaviour
{
    [Required]
    public AttackStatsInfoBar InfoBarTemplate;

    [Required]
    public Transform Container;

    public void Clear()
    {
        this.Container.DestroyChildren();
    }

    public void SetStats(List<CombatAttackInfo> infoList)
    {
        Clear();

        foreach (var info in infoList)
        {
            var bar = Instantiate(InfoBarTemplate);
            bar.transform.SetParent(Container, true);
            bar.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            bar.SetStats(info);
        }
    }
}
