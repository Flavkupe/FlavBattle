using FlavBattle.Combat;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FlavBattle.UI.Combat
{
    public class AttackStatsPanel : MonoBehaviour
    {
        [Required]
        public AttackStatsInfoBar InfoBarTemplate;

        [Required]
        public Transform Container;

        public void Clear()
        {
            this.Container.DestroyChildren();
        }

        public void SetStats(CombatTurnSummary summary)
        {
            Clear();

            var nonAllyActions = summary.Turns.Where(a => !a.IsAllyAbility).ToList();
            foreach (var action in nonAllyActions)
            {
                var bar = Instantiate(InfoBarTemplate);
                bar.transform.SetParent(Container, true);
                bar.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                bar.SetStats(action);
            }
        }
    }

}