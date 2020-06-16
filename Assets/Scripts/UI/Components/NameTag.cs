using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameTag : MonoBehaviour
{
    public TextMeshProUGUI TextNormies;
    public TextMeshProUGUI TextOfficer;
    public Image FrameNormies;
    public Image FrameOfficer;

    public void SetUnit(Unit unit)
    {
        FrameNormies.Hide();
        FrameOfficer.Hide();
        if (unit == null)
        {
            return;
        }

        if (unit.IsOfficer)
        {
            TextOfficer.text = unit.Info.Name;
            FrameOfficer.Show();
        }
        else
        {
            TextNormies.text = unit.Info.Name;
            FrameNormies.Show();
        }
    }
}
