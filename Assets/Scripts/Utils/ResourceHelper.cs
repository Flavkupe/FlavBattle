using FlavBattle.Entities.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourceHelper
{
    private enum AssetType
    {
        Units,
        Factions
    }

    private static Dictionary<AssetType, object> _cache = new Dictionary<AssetType, object>();

    public static UnitData[] Units => GetResources<BasicUnitData>(AssetType.Units);

    public static FactionData[] Factions=> GetResources<FactionData>(AssetType.Factions);

    private static TDataType[] GetResources<TDataType>(AssetType assetType) where TDataType : Object
    {
        if (_cache.ContainsKey(assetType))
        {
            var val = _cache[assetType];
            Debug.Assert(val is TDataType[], "Requested assets are not of specified type!");
            return val as TDataType[];
        }

        var resources = MiscUtils.LoadAssets<TDataType>(assetType.ToString());
        _cache[assetType] = resources;
        return resources;
    }
}


