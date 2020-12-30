using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CombatAttackInfo
{
    public bool LeftSource => Source.Combatant.Left;
    public bool IsAllyAbility => TargetInfo.AffectsAllies();

    public ComputedAttackInfo Source { get; set; }
    public BattleStatus State { get; set; }
    public CombatTargetInfo TargetInfo { get; set; }
    public CombatAbilityData Ability { get; set; }
    public List<ComputedAttackInfo> Targets { get; set; }

}

public class ComputedAttackInfo
{
    public int Attack { get; set; }
    public int Defense { get; set; }
    public Combatant Combatant { get; set; }
}

public class ComputedAttackResultInfo
{
    public ComputedAttackResultInfo(Combatant target)
    {
        Target = target;
    }

    public ComputedAttackResultInfo()
    {
    }

    /// <summary>
    /// Morale damage caused directly by morale-affecting abilities
    /// </summary>
    public int? DirectMoraleDamage { get; set; }

    /// <summary>
    /// Morale damage caused by external factors
    /// </summary>
    public int? IndirectMoraleDamage { get; set; }

    public int? AttackDamage { get; set; }
    public int? ArmyMoraleDamage { get; set; }

    public Combatant Target { get; private set; }
}
