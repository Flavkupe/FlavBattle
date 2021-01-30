using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IBattleState
{
    void Update(BattleStatus state);
    bool ShouldUpdate(BattleStatus state);
}

public abstract class BattleStateBase : IBattleState
{
    public abstract bool ShouldUpdate(BattleStatus state);
    protected abstract IEnumerator Run(BattleStatus state);

    protected MonoBehaviour _owner;

    public BattleStateBase(MonoBehaviour owner)
    {
        _owner = owner;
    }

    public void Update(BattleStatus state)
    {
        if (state.TurnExecuting)
        {
            return;
        }

        state.TurnExecuting = true;
        _owner.StartCoroutine(Execute(state));
    }

    private IEnumerator Execute(BattleStatus state)
    {
        yield return Run(state);
        state.TurnExecuting = false;
    }

    /// <summary>
    /// Hides all UI for combat (Buttons, backdrop, etc)
    /// </summary>
    protected IEnumerator HideCombatUI(BattleStatus state)
    {
        state.BattleUIPanel.Hide();
        yield return state.BattleDisplay.HideCombatScene();
    }
}
