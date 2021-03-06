﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Combat
{
    public class CombatBuffIcon : MonoBehaviour
    {
        public enum BuffType
        {
            MoraleShield,
            BlockShield,
        }

        /// <summary>
        /// Type of buff this represents
        /// </summary>
        public BuffType Type;
    }
}
