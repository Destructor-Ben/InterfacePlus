namespace InterfacePlus.Content.CraftTree.Sources;

// We don't know anything about how to obtain this
public record DeadEndSource(int ItemType, int Stack) : INodeSource
{
    public Item DrawnItem => new Item(ItemType, Stack);

    public IEnumerable<INodeSource> GetChildren()
    {
        return [];
    }
}
