using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleDisplay : MonoBehaviour
{
    public CombatFormation LeftFormation;
    public CombatFormation RightFormation;

    public GameObject LeftSide;
    public GameObject RightSide;

    public AnimatedSpin VictorySign;
    public AnimatedSpin DefeatSign;

    public float BackdropShiftSpeed = 10.0f;

    private float _backdropMinY = -6.25f;
    private float _backdropMaxY = 6.25f;

    // Start is called before the first frame update
    void Start()
    {
        // LeftSide.Hide();
        // RightSide.Hide();
    }

    // Update is called once per frame
    void Update()
    {   
    }

    public Coroutine ShowCombatEndSign(bool victory)
    {
        return StartCoroutine(ShowCombatEndSignInternal(victory));
    }

    public Coroutine InitializeCombatScene(IArmy left, IArmy right)
    {
        return StartCoroutine(InitializeCombatSceneInternal(left, right));
    }

    public Coroutine HideCombatScene()
    {
        return StartCoroutine(HideCombatSceneInternal());
    }

    private IEnumerator InitializeCombatSceneInternal(IArmy left, IArmy right)
    {
        LeftFormation.InitArmy(left);
        RightFormation.InitArmy(right);

        this.transform.position = Camera.main.transform.position.SetZ(0.0f);
        LeftSide.Show();
        RightSide.Show();

        LeftSide.transform.localPosition = LeftSide.transform.localPosition.SetY(_backdropMinY);
        RightSide.transform.localPosition = RightSide.transform.localPosition.SetY(_backdropMaxY);

        while (LeftSide.transform.localPosition.y < -0.1f ||
            RightSide.transform.localPosition.y > 0.1f)
        {
            LeftSide.transform.localPosition += Vector3.up * Time.deltaTime * BackdropShiftSpeed;
            RightSide.transform.localPosition += Vector3.up * Time.deltaTime * -BackdropShiftSpeed;
            yield return null;
        }

        LeftSide.transform.localPosition = LeftSide.transform.localPosition.SetY(0.0f);
        RightSide.transform.localPosition = RightSide.transform.localPosition.SetY(0.0f);
    }

    private IEnumerator HideCombatSceneInternal()
    {
        while (LeftSide.transform.localPosition.y > _backdropMinY ||
            RightSide.transform.localPosition.y < _backdropMaxY)
        {
            LeftSide.transform.localPosition -= Vector3.up * Time.deltaTime * BackdropShiftSpeed;
            RightSide.transform.localPosition -= Vector3.up * Time.deltaTime * -BackdropShiftSpeed;
            yield return null;
        }

        LeftSide.transform.localPosition = LeftSide.transform.localPosition.SetY(_backdropMinY);
        RightSide.transform.localPosition = RightSide.transform.localPosition.SetY(_backdropMaxY);
        yield return new WaitForSeconds(1.0f);

        LeftFormation.ClearArmy();
        RightFormation.ClearArmy();
        this.Hide();
    }

    private IEnumerator ShowCombatEndSignInternal(bool victory)
    {
        var sign = victory ? VictorySign : DefeatSign;
        var instance = Instantiate(sign);
        instance.transform.position = this.transform.position;
        yield return instance.SpinAround();
    }
}
