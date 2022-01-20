using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Entities
{
    public interface IArmyAction
    {
        /// <summary>
        /// Whether the action is visible (example: near a Garrison)
        /// </summary>
        bool IsAvailable();

        /// <summary>
        /// Whether the action is visible but blocked (eg: not enough
        /// command points to use).
        /// </summary>
        /// <returns></returns>
        bool IsLocked();

        /// <summary>
        /// Performs the action.
        /// </summary>
        void Perform();

        void SetArmy(Army army);

        Sprite Icon { get; }

        string TooltipMessage { get; }
    }

    public abstract class ArmyAction : IArmyAction
    {
        private Army _army;
        protected Army Army => _army;

        public ArmyAction()
        {
        }

        /// <summary>
        /// A default icon to set for the ArmyAction.
        /// </summary>
        public ArmyAction(Sprite icon)
        {
            _icon = icon;
        }

        private Sprite _icon = null;

        /// <summary>
        /// Icon shown for ArmyAction
        /// </summary>
        public virtual Sprite Icon => _icon;

        public void SetArmy(Army army)
        {
            _army = army;
        }

        public abstract string TooltipMessage { get; }

        public abstract bool IsAvailable();

        public abstract bool IsLocked();

        public abstract void Perform();
    }
}
