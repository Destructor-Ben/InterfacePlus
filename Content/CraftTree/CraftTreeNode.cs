namespace InterfacePlus.Content.CraftTree;

public record struct CraftTreeNode(
    // TODO: these should be put into a separate type (since we want to support more than just items: [[
    int ItemType,
    int Count,
    // TODO: ]]
    int TreeWidth, // Largest number of children of a single node in this subtree
    int TreeHeight, // Depth of this subtree
    List<CraftTreeNode> Children
);
