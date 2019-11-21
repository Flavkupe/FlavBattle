using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetIcons.Example
{
	[CreateAssetMenu (menuName = "AssetIcons-Example/Prop")]
	public class Prop : ScriptableObject
	{
		[AssetIcon]
		public Sprite icon;
	}
}