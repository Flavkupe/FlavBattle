using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Town : MonoBehaviour, IDetectable
{
    [Required]
    public TownData Data;

    public DetectableType Type => DetectableType.Tile;

    public FactionData Faction;

    [Required]
    public SpriteRenderer BannerSprite;

    [Required]
    public RadialTimer Timer;

    private float _process = 0;

    public List<IArmy> _visitors = new List<IArmy>();

    public GameObject GetObject()
    {
        return this.gameObject;
    }

    private void Awake()
    {
        SetFaction(Faction);
    }

    private void Update()
    {
        if (GameEventManager.IsMapPaused)
        {
            return;
        }

        foreach (var army in this._visitors)
        {
            if (army.Faction.Faction != Faction?.Faction)
            {
                ArmyConquerTick(army);
            }
        }
    }

    private void ArmyConquerTick(IArmy army)
    {
        // TODO: based on leadership and morale
        var tick = TimeUtils.AdjustedGameDelta;
        
        if (_process + tick >= this.Data.RequiredToTake)
        {
            Conquered(army);
        }
        else
        {
            SetProcess(_process + tick);
        }
    }

    private void Conquered(IArmy army)
    {
        SetProcess(0.0f);
        SetFaction(army.Faction);

        Sounds.Play(Data.GoodConqueredClip);
        var floater = Instantiate(Data.RedTextTemplate);
        floater.transform.SetParent(this.transform);
        floater.transform.localPosition = Vector3.zero;
        floater.SetText("Secured!");
    }

    public void Exited(IArmy army)
    {
        _visitors.Remove(army);
        if (_visitors.Count == 0)
        {
            SetProcess(0.0f);
        }
    }

    public void Entered(IArmy army)
    {
        _visitors.Add(army);
    }

    public void SetProcess(float process)
    {
        _process = process;
        if (Data.RequiredToTake > 0.0f)
        {
            Timer.SetPercentage(_process / Data.RequiredToTake);
        }
    }

    public void SetFaction(FactionData data)
    {
        Faction = data;
        if (data == null)
        {
            BannerSprite.gameObject.Hide();
            BannerSprite.sprite = null;
        }
        else
        {
            BannerSprite.gameObject.Show();
            BannerSprite.sprite = data.Flag;
        }
    }
}
