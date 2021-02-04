using UnityEngine;
using UnityEngine.UI;

public class BattleCommandButton : MonoBehaviour
{
    private Unit _current;

    [SerializeField]
    private Text _commandsCounterLabel; 

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetOfficer(Unit unit)
    {
        _commandsCounterLabel.text = unit.Info.CurrentStats.Commands.ToString();
        if (_current != null)
        {
            unit.Info.StatChanged -= HandleStatChanged;
        }

        _current = unit;
        unit.Info.StatChanged += HandleStatChanged;
    }

    private void HandleStatChanged(object o, UnitStatChangeEventArgs e)
    {
        _commandsCounterLabel.text = e.Current.Commands.ToString();
    }
}
