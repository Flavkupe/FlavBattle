using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCursor : MonoBehaviour
{
    private TilemapManager _map;
    private SpriteRenderer _sprite;

    // Start is called before the first frame update
    void Start()
    {
        _map = FindObjectOfType<TilemapManager>();
        _sprite = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        var mousePos = MiscUtils.MouseToWorldPoint();
        var pos = _map.GetGridTileAtWorldPos(mousePos);
        if (pos != null)
        {
            if (!_sprite.gameObject.activeInHierarchy)
            {
                _sprite.gameObject.SetActive(true);
            }

            this.transform.position = pos.ToWorldPos();
        }
        else if (_sprite.gameObject.activeInHierarchy)
        {
            _sprite.gameObject.SetActive(false);
        }
    }
}
