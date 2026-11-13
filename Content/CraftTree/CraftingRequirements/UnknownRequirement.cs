namespace InterfacePlus.Content.CraftTree.CraftingRequirements;

public record UnknownRequirement(Condition Condition) : ICraftingRequirement
{
    public Item DrawnItem => new(ItemID.None);
    public string Description => Condition.Description.Value;

    public void DrawExtra(SpriteBatch spriteBatch, Vector2 slotTopLeft, Vector2 slotSize)
    {
        DrawUnknownIcon(spriteBatch, slotTopLeft, slotSize);
    }

    public static void DrawUnknownIcon(SpriteBatch spriteBatch, Vector2 slotTopLeft, Vector2 slotSize)
    {
        var texture = Assets.Textures.UI.UnknownCraftingRequirement.Asset;
        spriteBatch.Draw(
            texture.Value,
            slotTopLeft + slotSize / 2 - texture.Size() / 2,
            Color.White
        );
    }
}
