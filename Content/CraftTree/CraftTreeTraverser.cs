namespace InterfacePlus.Content.CraftTree;

public static class CraftTreeTraverser
{
    private static int MaxDepth => 100;

    public static CraftTreeNode CreateCraftTree(int itemType, int count = 1, List<int> visitedItems = null, int depth = 0)
    {
        if (depth > MaxDepth)
            throw new Exception("Reached max depth"); // TODO: just return a node saying max depth reached instead...

        var self = new CraftTreeNode(itemType, count, 1, []);

        visitedItems ??= [];
        if (visitedItems.Contains(itemType))
            return self; // TODO: should probably fade duplicated items

        visitedItems.Add(itemType);

        // TODO: perhaps refactor this into a CraftTreeTraverser.GetItemSources() and an ItemSource struct, and use ItemSource in the CraftTreeNode
        // TODO: display all possible recipes for an item
        // TODO: additional item source info:
        // - shimmering
        // - item loot
        // - npc loot
        // - chest loot
        // - tile drops
        var recipes = Main.recipe.Where(r => r.createItem.type == itemType).ToList();
        if (recipes.Count <= 0)
            return self;

        // We have child nodes, so traverse them and add to children, then update the max width

        // TODO: handle recipe groups
        var recipe = recipes[0]; // TODO: handle multiple recipes
        self.TreeWidth = 0;

        foreach (var ingredient in recipe.requiredItemQuickLookup)
        {
            // TODO: temporary:
            if (ingredient.IsRecipeGroup)
                continue;

            var childNode = CreateCraftTree(ingredient.itemIdOrRecipeGroup, ingredient.stack, visitedItems, depth + 1);
            self.Children.Add(childNode);
            self.TreeWidth += childNode.TreeWidth;
        }

        return self;
    }
}
