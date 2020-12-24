using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(ScenarioObjectives))]
public class ScenarioManager : SingletonObject<ScenarioManager>
{
    [Required]
    public ObjectivePanel ObjectivePanel;

    [Required]
    public GameEventManager GameEventManager;

    [Required]
    public AnimatedSpin VictorySign;

    private ThrottleTimer _throttle = new ThrottleTimer(2.0f);

    private ScenarioObjectives _objectives;

    private ScenarioObjective _currentObjective;

    private void Awake()
    {
        this.SetSingleton(this);

        _objectives = GetComponent<ScenarioObjectives>();
        VictorySign.Hide();
    }

    private void Start()
    {
        _currentObjective = _objectives.GetNextObjective();

        if (_currentObjective == null)
        {
            Debug.LogError("No Objective found!!");
            return;
        }

        StartCoroutine(ShowObjectivesUI());
    }

    void Update()
    {
        // Throttle the objective checks to avoid doing too many checks
        if (!_throttle.Tick() || GameEventManager.IsMapPaused)
        {
            return;
        }

        if (_currentObjective != null)
        {
            if (_currentObjective.Check())
            {
                _currentObjective = _objectives.GetNextObjective();
                if (_currentObjective == null)
                {
                    // No more objectives
                    this.VictoryAchieved();
                    return;
                }
                else
                {
                    StartCoroutine(ShowObjectivesUI());
                }
            }
        }
    }

    private IEnumerator ShowObjectivesUI()
    {
        GameEventManager.TriggerMapEvent(MapEventType.MapPaused);
        ObjectivePanel.SetObjective(_currentObjective);
        yield return ObjectivePanel.Animate();
        GameEventManager.TriggerMapEvent(MapEventType.MapUnpaused);
    }

    private void VictoryAchieved()
    {
        Debug.Log("Victory achieved");
        GameEventManager.TriggerMapEvent(MapEventType.MapPaused);
        VictorySign.Show();
        VictorySign.SpinAround();
    }
}

