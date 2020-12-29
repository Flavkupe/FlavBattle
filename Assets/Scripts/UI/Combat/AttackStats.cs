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

    public void SetStats(List<CombatAttackInfoPair> pairs)
    {
        this.Container.DestroyChildren();

        foreach (var pair in pairs)
        {
            var bar = Instantiate(InfoBarTemplate);
            bar.transform.SetParent(Container, true);
            bar.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            bar.SetStats(pair);
        }
    }
}
