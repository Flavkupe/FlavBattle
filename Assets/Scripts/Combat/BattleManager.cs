using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public BattleDisplay BattleDisplay;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Coroutine StartCombat(Army player, Army enemy)
    {
        return StartCoroutine(StartCombatInternal(player, enemy));
    }

    private IEnumerator StartCombatInternal(Army player, Army enemy)
    {
        yield return BattleDisplay.InitializeCombatScene(player, enemy);
    }
}
