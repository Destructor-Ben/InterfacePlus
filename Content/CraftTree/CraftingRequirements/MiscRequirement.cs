namespace InterfacePlus.Content.CraftTree.CraftingRequirements;

public record MiscRequirement(Condition Condition, int ItemType) : ICraftingRequirement
{
    public Item DrawnItem => new(ItemType);
    public string Description => Condition.Description.Value;
}
