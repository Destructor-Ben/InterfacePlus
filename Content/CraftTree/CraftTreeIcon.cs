namespace InterfacePlus.Content.CraftTree;

public record CraftTreeIcon(
    Vector2 Position,
    CraftTreeIcon Parent,
    List<CraftTreeIcon> Children,
    int ItemType,
    Item DrawnItem
);
