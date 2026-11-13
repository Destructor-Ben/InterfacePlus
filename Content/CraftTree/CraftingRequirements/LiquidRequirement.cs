namespace InterfacePlus.Content.CraftTree.CraftingRequirements;

public record LiquidRequirement(Condition Condition, short LiquidType) : ICraftingRequirement
{
    public Item DrawnItem => new(DrawnItemType);
    public string Description => Condition.Description.Value;

    public int DrawnItemType => LiquidType switch
    {
        LiquidID.Water => ItemID.WaterBucket,
        LiquidID.Honey => ItemID.HoneyBucket,
        LiquidID.Lava => ItemID.LavaBucket,
        LiquidID.Shimmer => ItemID.BottomlessShimmerBucket,
        _              => throw new ArgumentOutOfRangeException(),
    };
}
