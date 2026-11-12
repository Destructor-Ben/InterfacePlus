namespace InterfacePlus.Content.CraftTree;

public record CraftTreeIcon(
    CraftTreeNode Node,
    Vector2 Position,
    CraftTreeIcon Parent,
    List<CraftTreeIcon> Children
);
