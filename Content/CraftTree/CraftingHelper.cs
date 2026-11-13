using InterfacePlus.Content.CraftTree.CraftingRequirements;

namespace InterfacePlus.Content.CraftTree;

public static class CraftingHelper
{
    public static IEnumerable<ICraftingRequirement> GetCraftingRequirements(Recipe recipe)
    {
        if (recipe.requiredTile >= TileID.Dirt)
            yield return new CraftingStationRequirement(recipe.requiredTile);

        foreach (var condition in recipe.Conditions)
        {
            if (condition == Condition.NearWater)
                yield return new LiquidRequirement(condition, LiquidID.Water);
            else if (condition == Condition.NearHoney)
                yield return new LiquidRequirement(condition, LiquidID.Honey);
            else if (condition == Condition.NearLava)
                yield return new LiquidRequirement(condition, LiquidID.Lava);
            else if (condition == Condition.NearShimmer)
                yield return new LiquidRequirement(condition, LiquidID.Shimmer);
            else if (condition == Condition.InSnow)
                yield return new MiscRequirement(condition, ItemID.SnowBlock);
            else if (condition == Condition.InGraveyard)
                yield return new MiscRequirement(condition, ItemID.Tombstone);
            else if (condition == Condition.NeedTorchGodsFavor)
                yield return new MiscRequirement(condition, ItemID.TorchGodsFavor);
            else
                yield return new UnknownRequirement(condition);
        }
    }

    // TODO: GetShimmerCraftingRequirements
}
