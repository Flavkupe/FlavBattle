using FlavBattle.State;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;

public class ScenarioManager : SingletonObject<ScenarioManager>
{
    [Required]
    public ObjectivePanel ObjectivePanel;

    [Required]
    public GameEventManager GameEventManager;

    [Required]
    public AnimatedSpin VictorySign;

    private ThrottleTimer _throttle = new ThrottleTimer(2.0f);

    [SerializeField]
    [Tooltip("First event that will happen when level starts.")]
    [Required]
    private GameEventBase _startingEvent;

    private ScenarioObjective _currentObjective;

    public IEnumerator SetAndShowObjective(ScenarioObjective objective)
    {
        if (objective == null)
        {
            Logger.Log(LogType.State, "Setting to null objective");
            yield break;
        }

        SetObjective(objective);
        yield return StartCoroutine(ShowObjectivesUI());
    }

    public void SetObjective(ScenarioObjective objective)
    {
        if (objective == null)
        {
            Logger.Log(LogType.State, "Setting to null objective");
            return;
        }

        _currentObjective = objective;
        _currentObjective.InitializeObjective();
    }

    private void Awake()
    {
        this.SetSingleton(this);
        VictorySign.Hide();
    }

    private void Start()
    {
        if (_startingEvent == null)
        {
            Debug.LogError("No starting event provided!");
            return;
        }

        GameEventManager.AddOrStartGameEvent(_startingEvent);
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
                var followup = _currentObjective.ObjectiveCompletedEvent;
                if (_currentObjective.IsVictoryCondition)
                {
                    this.VictoryAchieved();
                }

                _currentObjective = null;
                if (followup != null)
                {
                    GameEventManager.AddOrStartGameEvent(followup);
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
        Logger.Log(LogType.State, "Victory achieved");
        GameEventManager.TriggerMapEvent(MapEventType.MapPaused);
        VictorySign.Show();
        VictorySign.Animate();
    }
}

