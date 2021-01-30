using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Entities.Data
{
    [CreateAssetMenu(fileName = "Unit Data", menuName = "Custom/Units/Basic Unit Data", order = 1)]
    public class BasicUnitData : UnitData
    {
        /// <summary>
        /// Just for showing the assetIcon in the inspector; ignore this
        /// </summary>
        [AssetIcon]
        public override Sprite Icon => base.Icon;

        private string _name = "Unnamed";

        public override string ToString()
        {
            return $"Unit_{_name}";
        }

        public override Sprite RollPortrait()
        {
            if (this.Portraits.Length == 0)
            {
                Debug.LogWarning($"No portraits for unit {this.ToString()}!");
                return null;
            }

            return Portraits.GetRandom();
        }

        /// <summary>
        /// Sets a name
        /// </summary>
        public override string RollName()
        {
            // TODO: Data-driven names
            var _name = new List<string>
        {
            "Flavio",
            "Bob",
            "Blerb",
            "Bork",
            "Blahb"
        }.GetRandom();

            return _name;
        }


    }
}
