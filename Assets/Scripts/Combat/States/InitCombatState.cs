using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InitCombatState : BattleStateBase
{
    public InitCombatState(MonoBehaviour owner) : base(owner)
    {
    }

    /// <summary>
    /// Executes if there are no combatants in turn queue during combat phase
    /// </summary>
    public override bool ShouldUpdate(BattleStatus state)
    {
        return state.Stage == BattleStatus.BattleStage.InitCombat;
    }

    protected override IEnumerator Run(BattleStatus state)
    {
        yield return DoInitCombat(state);
    }

    private IEnumerator DoInitCombat(BattleStatus state)
    {
        var player = state.PlayerArmy;
        var enemy = state.OtherArmy;
        state.BattleDisplay.Show();
        yield return state.BattleDisplay.InitializeCombatScene(player, enemy);
        state.Combatants.AddRange(CreateCombatants(state, player, enemy, true));
        state.Combatants.AddRange(CreateCombatants(state, enemy, player, false));
        state.GameEventManager.TriggerCombatStartedEvent(player, enemy);

        // Init UI based on army setting
        state.BattleUIPanel.SetArmies(player, enemy);

        // Enable UI State
        state.BattleUIPanel.Show();

        foreach (var combatant in state.Combatants)
        {
            combatant.RightClicked += (object sender, Combatant e) =>
            {
                // first, ensure summaries are current before opening dialog
                state.BattleUIPanel.UnitStatsPanel.Open(e);
            };
        }

        yield return state.BattleUIPanel.AnimateInfoTextCallout("Combat Start");
        state.Stage = BattleStatus.BattleStage.PreCombatStart;
    }

    private IEnumerable<Combatant> CreateCombatants(BattleStatus state, IArmy allies, IArmy enemies, bool left)
    {
        var combatFormation = left ? state.BattleDisplay.LeftFormation : state.BattleDisplay.RightFormation;
        return allies.Formation.GetOccupiedPositionInfo().Select(a => new Combatant(
            combatFormation.GetFormationSlot(a.FormationPair.Row, a.FormationPair.Col))
        {
            Left = left,
            Unit = a.Unit,
            Row = a.FormationPair.Row,
            Col = a.FormationPair.Col,
            CombatFormation = combatFormation,
            Allies = allies,
            Enemies = enemies
        });
    }
}

