using FlavBattle.Combat;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FlavBattle.UI.Combat
{

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

        public void SetStats(CombatTurnUnitSummary summary)
        {
            var attackerInfo = summary.LeftSource ? LeftInfo : RightInfo;
            var defenderInfo = summary.LeftSource ? RightInfo : LeftInfo;

            LeftInfo.DestroyChildren();
            RightInfo.DestroyChildren();

            SetIconOn(attackerInfo, summary.FirstResult.Source.Unit.Info.Data.Icon);
            SetIconsOn(attackerInfo, SwordSprite, summary.FirstResult.Attack);

            foreach (var item in summary.Results)
            {
                SetIconOn(defenderInfo, item.Target.Unit.Info.Data.Icon);
                SetIconsOn(defenderInfo, ShieldSprite, item.Defense);
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
}