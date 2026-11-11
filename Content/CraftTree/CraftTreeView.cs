using Terraria.UI;

namespace InterfacePlus.Content.CraftTree;

// TODO: copy the camera control UI from the unfinished incremental mod
// TODO: give this a border
public class CraftTreeView : UIElement
{
    private static int IconVerticalShift => 100;
    private static int IconHorizontalShift => 100;
    private static bool EnableCentering => false; // TODO: fix

    public CraftTreeView()
    {
        OverflowHidden = true;
    }

    public void Reset()
    {
        RemoveAllChildren();
    }

    public void SetItem(int itemType)
    {
        var innerDimension = GetInnerDimensions();
        var position = new Vector2(innerDimension.Width / 2, IconVerticalShift);

        var craftTree = CraftTreeTraverser.CreateCraftTree(itemType);
        AddCraftTreeIcon(craftTree, position);
    }

    private void AddCraftTreeIcon(CraftTreeNode node, Vector2 position, CraftTreeIcon parent = null)
    {
        var icon = new CraftTreeIcon
        {
            ItemType = node.ItemType,
            ParentIcon = parent,
            Left = { Pixels = position.X },
            Top = { Pixels = position.Y },
        };
        Append(icon);

        var childPosition = position + Vector2.UnitY * IconVerticalShift;

        if (EnableCentering)
            childPosition.X -= node.TreeWidth / 2f;

        foreach (var child in node.Children)
        {
            AddCraftTreeIcon(child, childPosition, icon);
            childPosition.X += IconHorizontalShift * child.TreeWidth;
        }
    }
}
