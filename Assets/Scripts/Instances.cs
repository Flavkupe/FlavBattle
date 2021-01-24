using FlavBattle.Combat;
using FlavBattle.State;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instances : MonoBehaviour
{
    public static Instances Current { get; private set; }

    private void Awake()
    {
        Current = this;
    }

    [Serializable]
    public class ManagerRefs
    {
        public UIManager UI;

        public GameEventManager GameEvents;

        public TilemapManager TilemapManager;

        public GarrisonManager GarrisonManager;

        public ArmyManager ArmyManager;

        public BattleManager BattleManager;

        public SoundManager SoundManager;

        public GameResourceManager GameResourceManager;
    }

    public ManagerRefs Managers;
}
