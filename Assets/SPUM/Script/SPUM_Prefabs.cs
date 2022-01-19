﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SPUM_Prefabs : MonoBehaviour
{
    public float _version;
    public SPUM_SpriteList _spriteOBj;
    public bool EditChk;
    public string _code;
    public Animator _anim;

    public bool _horse;
    public string _horseString;

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

    public void PlayAnimation (int num)
    {
        switch(num)
        {
            case 0: //Idle
            _anim.SetFloat("RunState",0.25f);
            break;

            case 1: //Run
            _anim.SetFloat("RunState",0.5f);
            break;

            case 2: //Death
            _anim.SetTrigger("Die");
            _anim.SetBool("EditChk",EditChk);
            break;

            case 3: //Stun
            _anim.SetFloat("RunState",0.75f);
            break;

            case 4: //Attack Sword
            _anim.SetTrigger("Attack");
            _anim.SetFloat("AttackState",0.0f);
            _anim.SetFloat("NormalState",0.0f);
            break;

            case 5: //Attack Bow
            _anim.SetTrigger("Attack");
            _anim.SetFloat("AttackState",0.0f);
            _anim.SetFloat("NormalState",0.5f);
            break;

            case 6: //Attack Magic
            _anim.SetTrigger("Attack");
            _anim.SetFloat("AttackState",0.0f);
            _anim.SetFloat("NormalState",1.0f);
            break;

            case 7: //Skill Sword
            _anim.SetTrigger("Attack");
            _anim.SetFloat("AttackState",1.0f);
            _anim.SetFloat("SkillState",0.0f);
            break;

            case 8: //Skill Bow
            _anim.SetTrigger("Attack");
            _anim.SetFloat("AttackState",1.0f);
            _anim.SetFloat("SkillState",0.5f);
            break;

            case 9: //Skill Magic
            _anim.SetTrigger("Attack");
            _anim.SetFloat("AttackState",1.0f);
            _anim.SetFloat("SkillState",1.0f);
            break;

            case 10: //Block
            _anim.SetTrigger("Attack");
            _anim.SetFloat("AttackState", 0.0f);
            _anim.SetFloat("NormalState", 1.5f);
            break;

            case 11: //Static
            _anim.SetFloat("RunState", 0.0f);
            break;
        }
    }
}