
namespace FlavBattle.Components
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
        ProjectileFired = 2,
    }
}
