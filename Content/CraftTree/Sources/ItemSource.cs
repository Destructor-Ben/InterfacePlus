namespace InterfacePlus.Content.CraftTree.Sources;

public record ItemSource(int ItemType, int Stack) : INodeSource
{
    public Item DrawnItem => new(ItemType, Stack);

    public IEnumerable<INodeSource> GetChildren()
    {
        return CraftTreeTraverser.GetSourcesForItem(ItemType, Stack);
    }
}
