using UnityEngine;

public class FlowLayout : MonoBehaviour
{
    [SerializeField]
    private float _padding = 1.0f;

    public void AddObject(GameObject obj)
    {
        if (!obj.HasComponent<SpriteRenderer>())
        {
            Debug.LogWarning($"Object {obj.name} does not have a SpriteRenderer - ignoring");
            return;
        }

        obj.transform.SetParent(this.transform, false);
        UpdateLayout();
    }

    public void RemoveObject(GameObject obj, bool destroy = true)
    {
        if (obj != null)
        {
            foreach (var child in this.transform.GetComponentsInChildren<MonoBehaviour>())
            {
                if (child == null)
                {
                    continue;
                }

                var childObj = child.gameObject;
                if (childObj == obj)
                {
                    childObj.transform.SetParent(null);
                    Destroy(childObj);
                }
            }
        }

        UpdateLayout();
    }

    private void UpdateLayout()
    {
        // TODO: vertical too
        var x = 0.0f;
        foreach (var item in this.transform.GetComponentsInChildren<SpriteRenderer>())
        {
            if (item != null)
            {
                item.transform.localPosition = item.transform.localPosition.SetXY(x, 0.0f);
                var width = item.bounds.size.x;
                x += width + _padding;
            }
        }
    }
}
