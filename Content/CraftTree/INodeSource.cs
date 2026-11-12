namespace InterfacePlus.Content.CraftTree;

/// <summary>
/// Represents a the source of information (the displayed item and children) for a <see cref="CraftTreeNode"/>.
/// </summary>
// TODO: when tml updates to the C# version with unions, switch to them instead of interfaceslop
public interface INodeSource
{
    public Item DrawnItem { get; }

    public IEnumerable<INodeSource> GetChildren();

    public void DrawExtra(SpriteBatch spriteBatch, CraftTreeIcon icon) { }
}
