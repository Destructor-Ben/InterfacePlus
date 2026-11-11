using Terraria.UI;

namespace InterfacePlus.Content.CraftTree;

// TODO: copy the camera control UI from the unfinished incremental mod
public class CraftTreeView : UIElement
{
    private static int IconVerticalShift => 100;
    private static int IconHorizontalShift => 50;

    public CraftTreeView()
    {
        OverflowHidden = true;
    }

    public void Reset()
    {
        RemoveAllChildren();
    }

    // TODO: rewrite this eventually:
    // - traverse the tree and build the necessaryd ata for the UI
    // - THEN add the UI elements after that
    public void SetItem(int itemType)
    {
        var innerDimension = GetInnerDimensions();
        var position = new Vector2(innerDimension.Width / 2, IconVerticalShift);
        List<int> visitedItems = [itemType];
        AddItem(itemType, position, visitedItems);
    }

    // Returns the width of the sub-tree measured in number of items
    private int AddItem(int itemType, Vector2 position, List<int> visitedItems)
    {
        int maxWidth = 0;

        // TODO: display all possible recipes for an item
        // TODO: additional item source info:
        // - shimmering
        // - item loot
        // - npc loot
        // - chest loot
        // - tile drops
        var recipes = Main.recipe.Where(r => r.createItem.type == itemType).ToList();
        if (recipes.Count != 0)
        {
            int childCount = 0;
            var recipe = recipes[0];

            // TODO: proper layout
            var offset = new Vector2(0, IconVerticalShift);
            // TODO: handle recipe groups
            foreach (var ingredient in recipe.requiredItemQuickLookup)
            {
                var ingredientType = ingredient.itemIdOrRecipeGroup;

                // Ignore non-tree structures, knowing how to convert platforms to wood and vice-versa doesn't tell you how to craft anything with wood
                // TODO: perhaps still add, but fade the icon out so it's clear that its duplicated:
                // - and also make sure that we don't continue to add items, otherwise we get a stack overflow
                if (visitedItems.Contains(ingredientType))
                    continue;

                visitedItems.Add(ingredientType);

                int subChildCount = AddItem(ingredientType, position + offset, visitedItems);
                offset.X += IconHorizontalShift * subChildCount;
                childCount += 1;
                maxWidth = Math.Max(maxWidth, Math.Max(childCount, subChildCount));
            }
        }

        // TODO: show crafting requirements
        // TODO: tooltip on hover
        // TODO: required item stack count
        Append(new CraftTreeIcon
        {
            ItemType = itemType,
            Left = { Pixels = position.X },
            Top = { Pixels = position.Y },
        });

        return maxWidth;
    }
}
