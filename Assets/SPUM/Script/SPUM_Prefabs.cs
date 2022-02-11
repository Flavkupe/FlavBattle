using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FlavBattle.Components;

public class SPUM_Prefabs : MonoBehaviour
{
    public float _version;
    public SPUM_SpriteList _spriteOBj;
    public bool EditChk;
    public string _code;
    public Animator _anim;

    public bool _horse;
    public string _horseString;

    public float Speed => _anim?.speed ?? 1.0f;

    /// <summary>
    /// Checks that the current state matches the specified one.
    /// 
    /// NOTE: This uses Tags to check the state. Tags should be like:
    /// AttackState
    /// RunState
    /// DieState
    /// 
    /// </summary>
    /// <param name="stateName"></param>
    /// <returns></returns>
    public bool IsInState(string stateName)
    {
        return _anim.GetCurrentAnimatorStateInfo(0).IsTag(stateName);
    }

    public void SetSpeed(float speed = 1.0f)
    {
        _anim.speed = speed;
    }

    public void PlayAnimation(UnitAnimatorTrigger trigger, float speed = 1.0f)
    {
        SetSpeed(speed);
        switch (trigger)
        {
            case UnitAnimatorTrigger.Idle:
                _anim.SetFloat("RunState", 0.25f);
                break;
            case UnitAnimatorTrigger.Run:
                _anim.SetFloat("RunState", 0.5f);
                break;

            case UnitAnimatorTrigger.Die:
                _anim.SetTrigger("Die");
                _anim.SetBool("EditChk", EditChk);
                break;

            case UnitAnimatorTrigger.Melee:
                _anim.SetTrigger("Attack");
                _anim.SetFloat("AttackState", 0.0f);
                _anim.SetFloat("NormalState", 0.0f);
                break;

            case UnitAnimatorTrigger.ShootBow:
                _anim.SetTrigger("Attack");
                _anim.SetFloat("AttackState", 0.0f);
                _anim.SetFloat("NormalState", 0.5f);
                break;

            case UnitAnimatorTrigger.AttackMagic:
                _anim.SetTrigger("Attack");
                _anim.SetFloat("AttackState", 0.0f);
                _anim.SetFloat("NormalState", 1.0f);
                break;

            case UnitAnimatorTrigger.SkillSword:
                _anim.SetTrigger("Attack");
                _anim.SetFloat("AttackState", 1.0f);
                _anim.SetFloat("SkillState", 0.0f);
                break;

            case UnitAnimatorTrigger.SkillBow:
                _anim.SetTrigger("Attack");
                _anim.SetFloat("AttackState", 1.0f);
                _anim.SetFloat("SkillState", 0.5f);
                break;

            case UnitAnimatorTrigger.SpecialJump:
                _anim.SetTrigger("Attack");
                _anim.SetFloat("AttackState", 1.0f);
                _anim.SetFloat("SkillState", 1.0f);
                break;

            case UnitAnimatorTrigger.ShieldBlock:
                _anim.SetTrigger("Attack");
                _anim.SetFloat("AttackState", 0.0f);
                _anim.SetFloat("NormalState", 1.5f);
                break;

            case UnitAnimatorTrigger.Flinch:
                _anim.SetTrigger("Attack");
                _anim.SetFloat("AttackState", 0.0f);
                _anim.SetFloat("NormalState", 2.0f);
                break;

            case UnitAnimatorTrigger.Static:
            default:
                _anim.SetFloat("RunState", 0.0f);
                break;
        }
    }

    public void PlayAnimation(int num)
    {
        switch(num)
        {
            case 0: //Idle
                PlayAnimation(UnitAnimatorTrigger.Idle);
                break;

            case 1: //Run
                PlayAnimation(UnitAnimatorTrigger.Run);
                break;

            case 2: //Death
                PlayAnimation(UnitAnimatorTrigger.Die);
                break;

            case 3: //Stun
                // TODO
                PlayAnimation(UnitAnimatorTrigger.Stun);
                break;

            case 4: //Attack Sword
                PlayAnimation(UnitAnimatorTrigger.Melee);
                break;

            case 5: //Attack Bow
                PlayAnimation(UnitAnimatorTrigger.ShootBow);
                break;

            case 6: //Attack Magic
                PlayAnimation(UnitAnimatorTrigger.AttackMagic);
                break;

            case 7: //Skill Sword
                PlayAnimation(UnitAnimatorTrigger.SkillSword);
                break;

            case 8: //Skill Bow
                PlayAnimation(UnitAnimatorTrigger.SkillBow);
                break;

            case 9: //Skill Magic
                PlayAnimation(UnitAnimatorTrigger.SpecialJump);
                break;

            case 10: //Block
                PlayAnimation(UnitAnimatorTrigger.ShieldBlock);
                break;

            case 11: //Static
                PlayAnimation(UnitAnimatorTrigger.Static);
                break;

            case 12: //Flinch
                PlayAnimation(UnitAnimatorTrigger.Flinch);
                break;
        }
    }
}
