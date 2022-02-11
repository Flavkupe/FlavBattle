
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

        AttackMagic,
        SkillSword,
        SkillBow,
        Stun,
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

    public static class UnitAnimatorTriggerExtensions
    {
        public static UnitAnimatorState GetStateFromTrigger(this UnitAnimatorTrigger trigger)
        {
            switch (trigger)
            {
                case UnitAnimatorTrigger.Die:
                    return UnitAnimatorState.DieState;
                case UnitAnimatorTrigger.Idle:
                case UnitAnimatorTrigger.Run:
                case UnitAnimatorTrigger.Stun:
                    return UnitAnimatorState.RunState;

                case UnitAnimatorTrigger.ShootBow:
                case UnitAnimatorTrigger.ShieldBlock:
                case UnitAnimatorTrigger.Melee:
                case UnitAnimatorTrigger.Static:
                case UnitAnimatorTrigger.SpecialJump:
                case UnitAnimatorTrigger.Flinch:
                case UnitAnimatorTrigger.AttackMagic:
                case UnitAnimatorTrigger.SkillSword:
                case UnitAnimatorTrigger.SkillBow:
                default:
                    return UnitAnimatorState.AttackState;
            }
        }
    }
}
