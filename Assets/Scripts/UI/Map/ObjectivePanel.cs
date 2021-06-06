using NaughtyAttributes;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AnimatedMove))]
public class ObjectivePanel : MonoBehaviour
{
    [Required]
    public TMPro.TextMeshProUGUI Text;

    
    private AnimatedMove _animation;

    void Awake()
    {
        this.Hide();
    }

    public void SetObjective(ScenarioObjective objective)
    {
        Text.text = objective.DescriptionText;
    }

    public IEnumerator Animate()
    {
        if (_animation == null)
        {
            _animation = GetComponent<AnimatedMove>();
        }

        if (_animation == null)
        {
            Debug.LogError("Missing AnimatedMove component");
            yield break;
        }

        this.Show();
        yield return _animation.Animate();
        this.Hide();
    }
}
