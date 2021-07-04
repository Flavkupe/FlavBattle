using FlavBattle.State;
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

[Serializable]
public class ScenarioObjective
{
    [SerializeField]
    private GameEventBase _objectiveCompletedEvent;

    /// <summary>
    /// The Event that should happen after this objective is completed.
    /// </summary>
    public IGameEvent ObjectiveCompletedEvent => _objectiveCompletedEvent;

    public ScenarioObjectiveType Type;

    [Required]
    public string DescriptionText;

    [SerializeField]
    [Tooltip("Whether completing this objective should complete the level.")]
    private bool _isVictoryCondition = false;
    
    /// <summary>
    /// Whether completing this objective should complete the level.
    /// </summary>
    public bool IsVictoryCondition => _isVictoryCondition;

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
