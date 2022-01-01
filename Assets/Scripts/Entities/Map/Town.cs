using FlavBattle.State;
using NaughtyAttributes;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using FlavBattle.Resources;

public class Town : MonoBehaviour, IDetectable, IOwnedEntity
{
    public float RequiredToTake = 20.0f;

    public FactionData StartingFaction;

    [Required]
    public FloatingText RedTextTemplate;

    public DetectableType Type => DetectableType.Tile;

    public FactionData Faction { get; private set; }

    [Required]
    public SpriteRenderer BannerSprite;

    [Required]
    public RadialTimer Timer;

    private float _progress = 0;

    public List<IArmy> _visitors = new List<IArmy>();

    public GameObject GetObject()
    {
        return this.gameObject;
    }

    private void Awake()
    {
        SetFaction(StartingFaction);
    }

    private void Update()
    {
        if (GameEventManager.IsMapPaused)
        {
            return;
        }

        foreach (var army in this._visitors.ToList())
        {
            if (army.IsDestroyed)
            {
                // remove destroyed armies from lists of visitors
                _visitors.Remove(army);
            }
        }

        if (_visitors.Any(army => army.Faction.Faction == Faction?.Faction))
        {
            // if an army of the same faction is in the town, the town cannot be taken
            // and prgress is reset
            if (_progress != 0.0f)
            {
                SetProgress(0.0f);
            }

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
        
        if (_progress + tick >= this.RequiredToTake)
        {
            Conquered(army);
        }
        else
        {
            SetProgress(_progress + tick);
        }
    }

    private void Conquered(IArmy army)
    {
        SetProgress(0.0f);
        SetFaction(army.Faction);

        Sounds.Play(GRM.CommonSounds.FanfareSound);
        var floater = Instantiate(RedTextTemplate);
        floater.transform.SetParent(this.transform);
        floater.transform.localPosition = Vector3.zero;
        floater.SetText("Secured!");
    }

    public void Exited(IArmy army)
    {
        _visitors.Remove(army);
        if (_visitors.Count == 0)
        {
            SetProgress(0.0f);
        }
    }

    public void Entered(IArmy army)
    {
        _visitors.Add(army);
    }

    public void SetProgress(float progress)
    {
        _progress = progress;
        if (RequiredToTake > 0.0f)
        {
            Timer.SetPercentage(_progress / RequiredToTake);
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
