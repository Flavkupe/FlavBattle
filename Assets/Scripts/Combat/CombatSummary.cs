using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Combat
{
    /// <summary>
    /// Contains a list of each unit attacking at the same turn, and
    /// for each of them, contains a list of whom is getting attacked
    /// and the result of that attack.
    /// </summary>
    public class CombatTurnSummary
    {
        public BattleStatus State { get; set; }
        public Queue<CombatTurnUnitSummary> Turns { get; } = new Queue<CombatTurnUnitSummary>();

        public Combatant FirstCombatant => Turns.First()?.Source;

        /// <summary>
        /// Gets total sum of morale damage dealt
        /// </summary>
        public int ArmyMoraleDamage => Turns.Sum(a => a.Results.Sum(b => b.ArmyMoraleDamage));
    }

    /// <summary>
    /// Summary of attacks for a single unit, with all the enemies
    /// being attacked and the results of the attack.
    /// </summary>
    public class CombatTurnUnitSummary
    {
        public bool LeftSource => Source.Left;
        public bool IsAllyAbility => TargetInfo.AffectsAllies();

        public CombatTurnActionSummary FirstResult => Results.FirstOrDefault();

        public CombatAbilityData Ability { get; set; }
        public CombatTargetInfo TargetInfo { get; set; }
        public Combatant Source { get; set; }

        /// <summary>
        /// Result of each attack this unit has performed
        /// </summary>
        public List<CombatTurnActionSummary> Results { get; } = new List<CombatTurnActionSummary>();
    }

    /// <summary>
    /// Summary of a single action against a target this turn,
    /// including the results of the action. Can be friendly
    /// effect or attack.
    /// </summary>
    public class CombatTurnActionSummary
    {
        /// <summary>
        /// Who is attacking (or healing)
        /// </summary>
        public Combatant Source;

        public CombatAbilityData Ability;

        /// <summary>
        /// Who is this target of the attack (or heal)
        /// </summary>
        public Combatant Target;

        public Color? TileHighlight;

        /// <summary>
        /// The defense put up by the defender before the attack.
        /// </summary>
        public int Defense;

        /// <summary>
        /// The attack put up by the attacker before the attack.
        /// </summary>
        public int Attack;

        /// <summary>
        /// Morale damage caused directly by morale-affecting abilities
        /// </summary>
        public int DirectMoraleDamage { get; set; }

        /// <summary>
        /// Morale damage caused by external factors
        /// </summary>
        public int IndirectMoraleDamage { get; set; }

        public int AttackDamage { get; set; }
        public int ArmyMoraleDamage { get; set; }

        /// <summary>
        /// Whether attack was resisted due to def/att differences
        /// </summary>
        public bool ResistedAttack { get; set; }

        /// <summary>
        /// Whether attack was tanked using a shield unit
        /// </summary>
        public bool ShieldBlockedAttack { get; set; }

        /// <summary>
        /// Whether attack was tanked from high morale
        /// </summary>
        public bool MoraleBlockedAttack { get; set; }

        /// <summary>
        /// Sum of all recorded morale damages (includes both direct and indirect)
        /// </summary>
        public int TotalMoraleDamage => DirectMoraleDamage + IndirectMoraleDamage;

        /// <summary>
        /// Morale damage suffered by attacker as a result of some action
        /// (such as opponent blocking)
        /// </summary>
        public int SelfMoraleDamage { get; set; }
    }

}
