using UnityEngine;
using System;

/// <summary>
/// Used to specify that this member controls the ScriptableObject's Assets' Icon.
/// </summary>
[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method,
	AllowMultiple = false)]
public class AssetIconAttribute : PropertyAttribute
{

}