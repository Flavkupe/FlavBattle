public interface IOwnedEntity
{
    /// <summary>
    /// Faction owning the entity
    /// </summary>
    FactionData Faction { get; }
}

public static class IOwnedEntitiesExtensions
{
    public static bool IsPlayerOwned(this IOwnedEntity e)
    {
        return e.Faction?.IsPlayerFaction == true;
    }
}
