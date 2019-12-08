using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMain : MonoBehaviour
{
    public Coroutine PanTo(Vector3 position)
    {
        return StartCoroutine(PanToInternal(position));
    }

    private IEnumerator PanToInternal(Vector3 position)
    {
        yield return this.MoveTo(position, 20.0f);
    }
}
