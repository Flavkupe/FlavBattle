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

        [SerializeField]
        [Required]
        private FormationGridBase _uiTemplate;

        public void TestBuild()
        {
            for (int i = this.transform.childCount; i > 0; --i)
            {
                DestroyImmediate(this.transform.GetChild(0).gameObject);
            }

            if (_uiTemplate != null)
            {
                var grid = FormationUtils.CreateFormationGrid(_uiTemplate, _gapX, _gapY, FormationOrientation.BottomRight);
                grid.transform.SetParent(this.transform);
            }
        }
    }
}
