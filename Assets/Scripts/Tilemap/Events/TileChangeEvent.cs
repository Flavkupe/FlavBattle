using System;
using System.Collections;
using FlavBattle.State;
using NaughtyAttributes;
using UnityEngine;

namespace FlavBattle.Tilemap.Events
{
    public class TileChangeEvent : GameEventBase
    {
        [SerializeField]
        [Required]
        private UnityEngine.Tilemaps.Tilemap _tilemap;

        [SerializeField]
        private Vector3Int _tileCoords;

        [SerializeField]
        [Required]
        private UnityEngine.Tilemaps.TileBase _changeTo;

        public override bool IsAsyncEvent => false;

        public override IEnumerator DoEvent()
        {
            if (_tilemap != null)
            {
                try
                {
                    _tilemap.SetTile(_tileCoords, _changeTo);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            }

            yield return null;
        }

        public override bool EventPossible()
        {
            return true;
        }

        public override void PreStartEvent()
        {
        }
    }
}
