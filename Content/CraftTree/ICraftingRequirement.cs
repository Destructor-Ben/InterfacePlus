namespace InterfacePlus.Content.CraftTree;

public interface ICraftingRequirement
{
    public Item DrawnItem { get; }
    public string Description { get; }

    public void DrawExtra(SpriteBatch spriteBatch, Vector2 slotTopLeft, Vector2 slotSize) { }
}
