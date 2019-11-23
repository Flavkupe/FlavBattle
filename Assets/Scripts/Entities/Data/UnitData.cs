﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit Data", menuName = "Custom/Units/Unit Data", order = 1)]
public class UnitData : ScriptableObject
{
    [AssetIcon]
    public Sprite Icon;

    public Sprite Sprite;

    public string Name;
}
