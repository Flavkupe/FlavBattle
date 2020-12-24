using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public enum ScenarioObjectiveType
{
    TakeBuildings,
    DefeatArmies,
    SurviveMinutes,
}

public class ScenarioObjectives : MonoBehaviour
{
    public ScenarioObjective[] Objectives;

    private Queue<ScenarioObjective> _objectiveQueue = new Queue<ScenarioObjective>();

    void Awake()
    {
        foreach (var obj in Objectives)
        {
            _objectiveQueue.Enqueue(obj);
        }
    }

    public ScenarioObjective GetNextObjective()
    {
        if (_objectiveQueue.Count == 0)
        {
            return null;
        }

        // TODO: multiple simultaneous objectives
        var objective = _objectiveQueue.Dequeue();
        objective.InitializeObjective();
        return objective;
    }
}

[Serializable]
public class ScenarioObjective
{
    public ScenarioObjectiveType Type;

    // TODO: make this allow multiple starting objectives
    public bool StartingObjective = true;

    [Required]
    public string DescriptionText;

    private bool IsEntityCondition() => GetObjective()?.Fields.HasFlag(Fields.GameObjects) == true;
    private bool IsNumCondition() => GetObjective()?.Fields.HasFlag(Fields.Number) == true;

    private CheckBuildingsObjective _checkBuildingsObjective = new CheckBuildingsObjective();

    private IObjective GetObjective()
    {
        switch (Type)
        {
            case ScenarioObjectiveType.TakeBuildings:
                return _checkBuildingsObjective;
            default:
                Debug.LogError("Objective type not implemented");
                return null;
        }
    }

    public UnityEvent ObjectiveCompletedEvent;

    [AllowNesting]
    [ShowIf("IsEntityCondition")]
    public GameObject[] SpecificEntities;

    [AllowNesting]
    [ShowIf("IsNumCondition")]
    public int Amount;

    public void InitializeObjective()
    {
        var objective = GetObjective();
        if (objective != null)
        {
            objective.Initialize(this);
        }
    }

    public bool Check()
    {
        var objective = GetObjective();
        if (objective != null)
        {
            if (objective.CheckCompleted())
            {
                Debug.Log("Objective completed");
                ObjectiveCompletedEvent?.Invoke();
                return true;
            }
        }

        return false;
    }

    private class CheckBuildingsObjective : IObjective
    {
        private GameObject[] _entities;

        public Fields Fields => Fields.GameObjects;

        public bool CheckCompleted()
        {
            var owned = _entities.Select(a => a.GetComponent<IOwnedEntity>());
            if (owned.All(b => b.IsPlayerOwned()))
            {
                // All are buildings are player-owned
                return true;
            }

            return false;
        }

        public void Initialize(ScenarioObjective obj)
        {
            _entities = obj.SpecificEntities;
            if (_entities.Any(a => !a.HasComponent<IOwnedEntity>()))
            {
                Debug.LogError("Selected entity for condition is not IOwnedEntity");
                _entities = _entities.Where(a => a.HasComponent<IOwnedEntity>()).ToArray();
            }
        }
    }

    [Flags]
    private enum Fields
    {
        None = 0,
        GameObjects = 1,
        Number = 2,
    }

    private interface IObjective
    {
        /// <summary>
        /// Which fields should be configurable (for use in inspector)
        /// </summary>
        Fields Fields { get; }

        /// <summary>
        /// Initializes the objective and enables it so that it is active.
        /// </summary>
        /// <param name="obj"></param>
        void Initialize(ScenarioObjective obj);

        /// <summary>
        /// Check whether the objective has been completed.
        /// </summary>
        /// <returns></returns>
        bool CheckCompleted();
    }
}
