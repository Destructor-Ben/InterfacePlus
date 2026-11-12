namespace InterfacePlus.Content.CraftTree;

public record CraftTreeNode(INodeSource Source, List<CraftTreeNode> Children)
{
    /// <summary>
    /// The largest number of children of a single node in this subtree (including parent).
    /// </summary>
    public int TreeWidth { get; set; }

    /// <summary>
    /// The depth of this subtree (including parent).
    /// </summary>
    public int TreeHeight { get; set; }
};
