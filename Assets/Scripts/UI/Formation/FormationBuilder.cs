using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Formation
{
    public class FormationBuilder : MonoBehaviour
    {
        [SerializeField]
        private float _gapX = 1.0f;

        [SerializeField]
        private float _gapY = 1.0f;

        [HideIf("useObject")]
        [Required]
        public FormationGridBase _uiTemplate;

        [SerializeField]
        [Required]
        private bool useObject;

        [Required]
        [ShowIf("useObject")]
        public WithFormation _formation;

        [SerializeField]
        private FormationOrientation _orientation = FormationOrientation.BottomRight;

        public void TestBuild()
        {
            for (int i = this.transform.childCount; i > 0; --i)
            {
                DestroyImmediate(this.transform.GetChild(0).gameObject);
            }

            if (useObject && _formation != null)
            {
                BuildFormation();
            }
            else if (_uiTemplate != null)
            {
                BuildGrid();
            }
        }

        private void BuildFormation()
        {
            FormationUtils.PopulateFormation(this.transform, _formation, _orientation, _gapX, _gapY);
        }

        private void BuildGrid()
        {
            if (_uiTemplate != null)
            {
                var grid = FormationUtils.CreateFormationGrid(_uiTemplate, _gapX, _gapY, _orientation);
                grid.transform.SetParent(this.transform);
            }
        }
    }
}
