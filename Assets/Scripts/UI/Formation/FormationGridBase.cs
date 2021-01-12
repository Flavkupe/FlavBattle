using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Formation
{
    public abstract class FormationGridBase : MonoBehaviour, IFormationGrid
    {
        public abstract IFormationGridSlot CreateSlot();
    }
}
