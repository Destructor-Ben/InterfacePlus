namespace InterfacePlus.Content.CraftTree;

public record struct CraftTreeNode(
    // TODO: these should be put into a separate type (since we want to support more than just items: [[
    int ItemType,
    int Count,
    // TODO: ]]
    int TreeWidth, // TODO: explain this
    // TODO: add TreeHeight
    List<CraftTreeNode> Children
);
