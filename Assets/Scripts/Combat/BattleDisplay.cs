using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleDisplay : MonoBehaviour
{
    public CombatFormation LeftFormation;
    public CombatFormation RightFormation;

    public float BackdropShiftSpeed = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        LeftFormation.gameObject.SetActive(false);
        RightFormation.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {   
    }

    public Coroutine InitializeCombatScene(Army left, Army right)
    {
        return StartCoroutine(InitializeCombatSceneInternal(left, right));
    }

    private IEnumerator InitializeCombatSceneInternal(Army left, Army right)
    {
        LeftFormation.InitArmy(left);
        RightFormation.InitArmy(right);

        this.transform.position = Camera.main.transform.position.SetZ(0.0f);
        LeftFormation.gameObject.SetActive(true);
        RightFormation.gameObject.SetActive(true);

        LeftFormation.transform.localPosition = LeftFormation.transform.localPosition.SetY(-6.25f);
        RightFormation.transform.localPosition = RightFormation.transform.localPosition.SetY(6.25f);

        while (LeftFormation.transform.localPosition.y < -0.1f ||
            RightFormation.transform.localPosition.y > 0.1f)
        {
            LeftFormation.transform.localPosition += Vector3.up * Time.deltaTime * BackdropShiftSpeed;
            RightFormation.transform.localPosition += Vector3.up * Time.deltaTime * -BackdropShiftSpeed;
            yield return null;
        }

        LeftFormation.transform.localPosition = LeftFormation.transform.localPosition.SetY(0.0f);
        RightFormation.transform.localPosition = RightFormation.transform.localPosition.SetY(0.0f);
    }
}
