using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FlavBattle.UI.Combat
{
    public class CombatStancePanel : MonoBehaviour
    {
        [Required]
        [SerializeField]
        private Button NeutralStanceButton;

        [Required]
        [SerializeField]
        private Button DefensiveButton;

        [Required]
        [SerializeField]
        private Button OffensiveButton;

        public event EventHandler<FightingStance> OnStanceChangeClicked;

        // Start is called before the first frame update
        void Awake()
        {
            DefensiveButton.onClick.RemoveAllListeners();
            OffensiveButton.onClick.RemoveAllListeners();
            NeutralStanceButton.onClick.RemoveAllListeners();

            DefensiveButton.onClick.AddListener(() => OnStanceChangeClicked?.Invoke(this, FightingStance.Defensive));
            OffensiveButton.onClick.AddListener(() => OnStanceChangeClicked?.Invoke(this, FightingStance.Offensive));
            NeutralStanceButton.onClick.AddListener(() => OnStanceChangeClicked?.Invoke(this, FightingStance.Neutral));
        }
    }
}