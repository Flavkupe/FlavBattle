using FlavBattle.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Combat.Event
{
    /// <summary>
    /// Interface for game events that take place in combat
    /// </summary>
    public interface ICombatGameEvent : IGameEvent
    {
        void SetBattleContext(BattleStatus status);
    }
}
