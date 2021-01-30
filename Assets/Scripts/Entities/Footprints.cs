using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Footprints : MonoBehaviour
{
    [Tooltip("Template of gameobject to use to represent footprints")]
    [Required]
    [SerializeField]
    private GameObject FootprintNodeTemplate;

    [Tooltip("Template of gameobject to use to represent last node of path")]
    [Required]
    [SerializeField]
    private GameObject LastNodeTemplate;

    private bool _pathCreated = false;

    public void CreatePath(List<Vector3> nodeList)
    {
        if (_pathCreated)
        {
            Clear();
        }

        for (var i = 0; i < nodeList.Count - 1; i++)
        {
            var node = nodeList[i];
            var marker = Instantiate(FootprintNodeTemplate, this.transform);
            marker.transform.position = node;

            if (i + 1 < nodeList.Count)
            {
                var next = nodeList[i + 1];
                var midMarker = Instantiate(FootprintNodeTemplate, this.transform);
                midMarker.transform.position = (node + next) / 2.0f;
            }
        }

        var final = nodeList.LastOrDefault();
        if (final != null)
        {
            var marker = Instantiate(LastNodeTemplate, this.transform);
            marker.transform.position = final;
        }

        _pathCreated = true;
    }

    public void Clear()
    {
        this.transform.DestroyChildren();
        _pathCreated = false;
    }
}
