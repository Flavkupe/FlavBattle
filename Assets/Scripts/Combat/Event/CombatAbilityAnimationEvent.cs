using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Combat.Events
{

    /// <summary>
    /// Performing an attack or ability; this would be the animation itself,
    /// before damage or effects are applied
    /// </summary>
    public class CombatAbilityAnimationEvent : ICombatAnimationEvent
    {
        public CombatTurnUnitSummary _summary { get; private set; }

        private AnimationType _type;

        private MonoBehaviour _owner;

        public enum AnimationType
        {
            PreAttack,
            Ability,
            PostAttack,
        }

        public CombatAbilityAnimationEvent(MonoBehaviour owner, CombatTurnUnitSummary summary, AnimationType type)
        {
            _summary = summary;
            _type = type;
            _owner = owner;
        }

        public IEnumerator Animate()
        {
            var targets = _summary.Results.Select(a => a.Target).ToList();
            var combatant = _summary.Source;

            if (_type == AnimationType.PreAttack)
            {
                OverheadText();
                yield return PlayAdditionalAnimations(combatant, _summary.Ability.PreAttackAnimations, targets);
            }
            else if (_type == AnimationType.PostAttack)
            {
                yield return PlayAdditionalAnimations(combatant, _summary.Ability.PostAttackAnimations, targets);
            }
            else if (_type == AnimationType.Ability)
            {
                yield return PlayAbilityAnimation(_summary.Ability, _summary.TargetInfo, _summary.Results);
            }
        }

        /// <summary>
        /// If enabled, shows the name of the attack as floating text prior to attack.
        /// </summary>
        private void OverheadText()
        {
            var ability = _summary.Ability;
            if (ability.AnimateName)
            {
                var unit = _summary.Source.CombatUnit;
                unit.AnimateOverheadText(ability.Name, Color.blue);
            }
        }

        private IEnumerator PlayAbilityAnimation(CombatAbilityData ability, CombatTargetInfo targetInfo, List<CombatTurnActionSummary> results)
        {
            if (ability == null)
            {
                Logger.Log(LogType.Combat, "No ability available!");
                yield break;
            }
            else
            {
                Logger.Log(LogType.Combat, $"Using ability {ability.Name}");
            }

            // Targets
            Logger.Log(LogType.Combat, $"Targets: {string.Join(", ", results.Select(a => a.Target.Unit.Data.ClassName)) }");
            yield return AnimateAbilities(ability, results);
        }

        private IEnumerator AnimateAbilities(CombatAbilityData ability, List<CombatTurnActionSummary> results)
        {
            var routines = Routine.CreateEmptyRoutineSet(_owner, ability.AnimationSequence == CombatAnimationType.Parallel);
            foreach (var result in results)
            {
                var routine = AnimateAbility(result, ability).ToRoutine();
                routines.AddRoutine(routine);
            }

            yield return routines;
        }

        private IEnumerator AnimateAbility(CombatTurnActionSummary result, CombatAbilityData abilityData)
        {
            if (abilityData.VisualEffect == CombatAbilityVisual.None)
            {
                yield break;
            }

            var target = result.Target;
            var source = result.Source;

            CombatFormationSlot slot = null;
            if (target != null)
            {
                var tileColor = result.TileHighlight;
                slot = target.CombatFormationSlot;
                slot.Highlight(tileColor ?? Color.white);
            }

            // highlight source tile
            var sourceSlot = source.CombatFormationSlot;
            sourceSlot.Highlight(Color.cyan);

            //// Play animator animation first
            //if (abilityData.AnimatorTrigger != UnitAnimatorTrigger.None)
            //{
            //    var combatUnit = result.Source.CombatUnit;
            //    yield return combatUnit.PlayAnimatorToCompletion(abilityData.AnimatorTrigger);
            //}

            var obj = new GameObject("Ability");
            var ability = obj.AddComponent<CombatAbility>();

            ability.InitData(abilityData);

            ability.AttackAnimationStarting += (object o, EventArgs e) =>
            {
                // When/if the animation hits a target, play the anim
                PlaySoundClip(abilityData.PreHitSoundClips.GetRandom());
            };

            ability.AttackAnimatorCompleted += (object o, EventArgs e) =>
            {
                PlaySoundClip(abilityData.PostAnimatorSoundClips.GetRandom());
            };

            ability.TargetHit += (object o, EventArgs e) =>
            {
                // When/if the animation hits a target, play the anim
                _owner.StartCoroutine(AnimateAttackResults(result));
            };

            PlaySoundClip(abilityData.StartSoundClips.GetRandom());


            if (target != null)
            {
                yield return ability.StartTargetedAbility(source.CombatUnit, target.CombatFormationSlot.CurrentUnit);
            }
            else
            {
                yield return ability.StartUntargetedAbility(source.CombatUnit);
            }

            PlaySoundClip(abilityData.EndSoundClips.GetRandom());

            if (slot != null)
            {
                slot.ResetColor();
            }

            sourceSlot.ResetColor();
        }


        /// <summary>
        /// Plays CombatCharacterAnimations on targets or combatant, based on data.
        /// For exmaple, PreAttackAnimations or PostAttackAnimations, which is just
        /// the animations before or after combat.
        /// 
        /// Will run to completion if option is enabled. Otherwise will run in background.
        /// </summary>
        /// <param name="combatant">Unit whose turn it is</param>
        /// <param name="animationsData">Data for this set of animations, such as PreAttackAnimations or PostAttackAnimations</param>
        /// <param name="targets">Targets of ability, if any</param>
        private IEnumerator PlayAdditionalAnimations(Combatant combatant, CombatCharacterAnimations animationsData, List<Combatant> targets)
        {
            if (animationsData.Animations.Count() == 0)
            {
                yield break;
            }

            var routines = Routine.CreateEmptyRoutineSet(_owner, animationsData.Type == CombatAnimationType.Parallel);

            foreach (var animation in animationsData.Animations)
            {
                // TODO: duration type
                var animationList = new List<IPlayableAnimation>();
                if (animation.Target == CombatAnimationTarget.Self)
                {
                    var instance = GameObject.Instantiate(animation.Animation);
                    animationList.Add(instance);
                    instance.transform.position = combatant.CombatUnit.transform.position;
                }
                else if (animation.Target == CombatAnimationTarget.Target)
                {
                    foreach (var target in targets)
                    {
                        var instance = GameObject.Instantiate(animation.Animation);
                        animationList.Add(instance);
                        instance.transform.position = target.CombatUnit.transform.position;
                    }
                }

                foreach (var anim in animationList)
                {
                    if (animationsData.WaitForCompletion)
                    {
                        routines.AddRoutine(anim.PlayToCompletion().ToRoutine());
                    }
                    else
                    {
                        anim.PlayAnimation();
                    }
                }
            }

            if (animationsData.WaitForCompletion)
            {
                yield return routines.AsRoutine();
            }
        }

        private IEnumerator AnimateAttackResults(CombatTurnActionSummary summary)
        {
            if (summary.Target == null)
            {
                // Currently results with no target should probably just not animate
                yield break;
            }

            PlaySoundClip(summary.Ability.HitSoundClips.GetRandom());

            var anim = new CombatUnitAnimationEvent(_owner, summary);
            yield return anim.Animate();
        }

        private void PlaySoundClip(AudioClip clip)
        {
            if (clip != null)
            {
                Sounds.Play(clip);
            }
        }
    }
}