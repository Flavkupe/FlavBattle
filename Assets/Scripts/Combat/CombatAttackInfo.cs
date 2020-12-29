using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CombatAttackInfoPair
{
    public List<CombatAttackInfo> Left { get; set; }
    public List<CombatAttackInfo> Right { get; set; }
}

public class CombatAttackInfo
{
    public int Attack { get; set; }

    public int Defense { get; set; }

    public Combatant Combatant { get; set; }
}
