using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NaughtyAttributes;

public class BattleCommandMenu : MonoBehaviour
{
    /// <summary>
    /// Area where ability point icons are shown
    /// </summary>
    [Required]
    public VisualCounter CommandPointsArea;

    /// <summary>
    /// Where the commands are listed
    /// </summary>
    [Required]
    public Transform CommandArea;

    [Required]
    public BattleCommandMenuItem CommandItemTemplate;

    public void SetCommander(Unit commander)
    {
        CommandPointsArea.SetCount(commander.Info.CurrentStats.Command);
        CommandArea.DestroyChildren();

        var abilities = commander.Info.OfficerAbilities.Where(a =>
            a.TriggerType != OfficerAbilityTriggerType.Passive
        );

        foreach (var ability in abilities)
        {
            var item = Instantiate(CommandItemTemplate);
            item.SetAbility(ability);
            item.transform.SetParent(CommandArea, false);
        }
    }
}
