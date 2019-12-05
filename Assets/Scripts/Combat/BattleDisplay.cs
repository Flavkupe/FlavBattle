using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleDisplay : MonoBehaviour
{
    public CombatFormation LeftFormation;
    public CombatFormation RightFormation;

    public AnimatedSpin VictorySign;
    public AnimatedSpin DefeatSign;

    public float BackdropShiftSpeed = 10.0f;

    private float _backdropMinY = -6.25f;
    private float _backdropMaxY = 6.25f;

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

    public Coroutine ShowCombatEndSign(bool victory)
    {
        return StartCoroutine(ShowCombatEndSignInternal(victory));
    }

    public Coroutine InitializeCombatScene(Army left, Army right)
    {
        return StartCoroutine(InitializeCombatSceneInternal(left, right));
    }

    public Coroutine HideCombatScene()
    {
        return StartCoroutine(HideCombatSceneInternal());
    }

    private IEnumerator InitializeCombatSceneInternal(Army left, Army right)
    {
        LeftFormation.InitArmy(left);
        RightFormation.InitArmy(right);

        this.transform.position = Camera.main.transform.position.SetZ(0.0f);
        LeftFormation.gameObject.SetActive(true);
        RightFormation.gameObject.SetActive(true);

        LeftFormation.transform.localPosition = LeftFormation.transform.localPosition.SetY(_backdropMinY);
        RightFormation.transform.localPosition = RightFormation.transform.localPosition.SetY(_backdropMaxY);

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

    private IEnumerator HideCombatSceneInternal()
    {
        while (LeftFormation.transform.localPosition.y > _backdropMinY ||
            RightFormation.transform.localPosition.y < _backdropMaxY)
        {
            LeftFormation.transform.localPosition -= Vector3.up * Time.deltaTime * BackdropShiftSpeed;
            RightFormation.transform.localPosition -= Vector3.up * Time.deltaTime * -BackdropShiftSpeed;
            yield return null;
        }

        LeftFormation.transform.localPosition = LeftFormation.transform.localPosition.SetY(_backdropMinY);
        RightFormation.transform.localPosition = RightFormation.transform.localPosition.SetY(_backdropMaxY);
        yield return new WaitForSeconds(1.0f);

        LeftFormation.ClearArmy();
        RightFormation.ClearArmy();
    }

    private IEnumerator ShowCombatEndSignInternal(bool victory)
    {
        var sign = victory ? VictorySign : DefeatSign;
        var instance = Instantiate(sign);
        instance.transform.position = this.transform.position;
        yield return instance.SpinAround();
    }
}
