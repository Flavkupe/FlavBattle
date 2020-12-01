using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NaughtyAttributes;

public class AbilityNameCallout : MonoBehaviour
{
    [Required]
    public TextMeshProUGUI Text;

    private Vector3 _initialPos;

    // Start is called before the first frame update
    void Start()
    {
        _initialPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetData(OfficerAbilityData data)
    {
        this.Text.text = data.Name;
    }

    public void SetData(CombatAbilityData data)
    {
        this.Text.text = data.Name;
    }

    public IEnumerator Animate()
    {
        this.Show();
        this.transform.position = _initialPos;
        var time = 0.0f;
        while (time < 1.0f)
        {
            time += Time.deltaTime;
            this.transform.position = Vector3.MoveTowards(this.transform.position, this.transform.position + Vector3.up, Time.deltaTime / 2);
            yield return null;
        }

        this.Hide();
    }
}
