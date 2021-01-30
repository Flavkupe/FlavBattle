using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Combat
{
    public class StatsBuff
    {
        public StatsBuff(Combatant target, UnitStats buff, int duration = 0)
        {
            Buff = buff;
            if (duration > 0)
            {
                _hasDuration = true;
                _duration = duration;
            }
            else
            {
                _hasDuration = false;
            }
        }

        public UnitStats Buff { get; private set; }

        private int _duration = 0;
        private bool _hasDuration = false;

        public bool Expired => _hasDuration && _duration <= 0;

        /// <summary>
        /// Reduces duration by one if this has a duration.
        /// </summary>
        public void TickDuration()
        {
            if (_hasDuration)
            {
                _duration--;
            }
        }
    }
}
