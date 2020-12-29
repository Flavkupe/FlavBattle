using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackStatsInfoBar : MonoBehaviour
{
    [Required]
    public Image IconTemplate;

    [Required]
    public Transform LeftInfo;

    [Required]
    public Transform RightInfo;

    [Required]
    public Sprite SwordSprite;

    [Required]
    public Sprite ShieldSprite;

    public void SetStats(CombatAttackInfoPair pair)
    {
        var left = pair.Left;
        var right = pair.Right;

        // LeftInfo.DestroyChildren();
        // RightInfo.DestroyChildren();

        foreach (var item in left)
        {
            SetIconOn(LeftInfo, item.Combatant.Unit.Info.Data.Icon);
            SetIconsOn(LeftInfo, SwordSprite, item.Attack);
            SetIconsOn(LeftInfo, ShieldSprite, item.Defense);
        }

        foreach (var item in right)
        {
            SetIconsOn(RightInfo, SwordSprite, item.Attack);
            SetIconsOn(RightInfo, ShieldSprite, item.Defense);
            SetIconOn(RightInfo, item.Combatant.Unit.Info.Data.Icon);
        }
    }

    private void SetIconsOn(Transform panel, Sprite sprite, int times)
    {
        for (int i = 0; i < times; i++)
        {
            SetIconOn(panel, sprite);
        }
    }

    private void SetIconOn(Transform panel, Sprite sprite)
    {
        var icon = Instantiate(IconTemplate);
        icon.sprite = sprite;
        icon.transform.SetParent(panel);
        icon.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }
}
