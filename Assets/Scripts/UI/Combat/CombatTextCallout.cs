using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NaughtyAttributes;

public class CombatTextCallout : MonoBehaviour
{
    [Required]
    public TextMeshProUGUI Text;

    private Vector3 _initialPos;

    [Tooltip("How long this callout is alive for")]
    public float AnimationTime = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        _initialPos = this.transform.position;
        this.Hide();
    }

    public void SetData(OfficerAbilityData data)
    {
        this.Text.text = data.Name;
    }

    public void SetData(CombatAbilityData data)
    {
        this.Text.text = data.Name;
    }

    public void SetText(string text)
    {
        this.Text.text = text;
    }

    public IEnumerator Animate()
    {
        this.Show();
        this.transform.position = _initialPos;
        var time = 0.0f;
        while (time < AnimationTime)
        {
            var delta = TimeUtils.FullAdjustedGameDelta;
            time += delta;
            this.transform.position = Vector3.MoveTowards(this.transform.position, this.transform.position + Vector3.up, delta / 2);
            yield return null;
        }

        this.Hide();
    }
}
