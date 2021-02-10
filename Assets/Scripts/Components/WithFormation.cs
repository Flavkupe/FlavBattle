using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Formation
{
    /// <summary>
    /// Component that indicates that this gameobject has
    /// a formation
    /// </summary>
    public class WithFormation : MonoBehaviour, IFormation
    {
        public FormationRow Row { get => _row; set => _row = value; }
        public FormationColumn Col { get => _col; set => _col = value; }

        public MonoBehaviour Instance => this;

        [SerializeField]
        private FormationRow _row;

        [SerializeField]
        private FormationColumn _col;

        public FormationPair Pair => FormationPair.From(Row, Col);

        public bool Matches(FormationPair pair) => pair.Equals(Row, Col);
    }
}
