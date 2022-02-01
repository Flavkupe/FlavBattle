using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Combat
{
    public enum UnitAnimatorTrigger
    {
        Idle,
        Die,
        ShootBow,
        ShieldBlock,
        Melee,
        Static,
        SpecialJump,
        Run,
        Flinch,
    }

    public enum UnitAnimatorState
    {
        RunState,
        AttackState,
        DieState,
    }

    public enum UnitAnimatorEvent
    {
        AttackHit = 1,
    }
}
