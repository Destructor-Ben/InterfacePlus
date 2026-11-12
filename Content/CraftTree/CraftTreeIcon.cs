namespace InterfacePlus.Content.CraftTree;

public record CraftTreeIcon(
    Vector2 Position,
    int ItemType,
    CraftTreeIcon Parent,
    List<CraftTreeIcon> Children
);
