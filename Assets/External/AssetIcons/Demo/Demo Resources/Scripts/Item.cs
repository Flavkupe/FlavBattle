using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetIcons.Example
{
	[CreateAssetMenu (menuName = "AssetIcons-Example/Item")]
	public class Item : ScriptableObject
	{
		[Header ("General")]

		public string Name = "New Item";
		[AssetIcon]
		public Sprite Icon;

		[Space]
		public int StackSize = 1;
		public float Value = 100.0f;
	}
}