using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingAbilitiesManager : MonoBehaviour
{
    public GameObject Left;

    public GameObject Right;

    public CombatAbilityData AbilityData;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoAbility()
    {
        var obj = new GameObject("Ability");
        var ability = obj.AddComponent<CombatAbility>();

        ability.InitData(AbilityData);
        ability.StartTargetedAbility(Left, Right);
    }
}
