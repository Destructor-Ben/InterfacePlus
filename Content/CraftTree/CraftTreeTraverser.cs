using InterfacePlus.Content.CraftTree.Sources;

namespace InterfacePlus.Content.CraftTree;

public static class CraftTreeTraverser
{
    private static int MaxDepth => 100;

    // TODO: add additional sources:
    // - item loot
    // - npc loot
    // - npc shops
    // - chest loot
    public static IEnumerable<INodeSource> GetSourcesForItem(int itemType, int stack)
    {
        List<INodeSource> sources = [];

        sources.AddRange(Main.recipe.Where(r => r.createItem.type == itemType).Select(r => new RecipeSource(r)));

        // ItemID.Sets.ShimmerXX
        // TODO: npcs (vanilla only has duplicates of the critter shimmerings)
        // NPCID.Sets.ShimmerXX
        // TODO: ItemId.Sets.SHimmerCountsAs
        sources.AddRange(ItemID.Sets.ShimmerTransformToItem.Where((input, output) => output == itemType).Select(i => new ItemSource(i, stack)));

        return sources;
    }

    // TODO: display all possible recipes for an item? or allow selecting the recipe? or combine the recipes?
    public static CraftTreeNode CreateCraftTree(int itemType, int stack = 1)
    {
        var sources = GetSourcesForItem(itemType, stack).ToArray();
        return sources.Length <= 0 ? null : CreateCraftTree(sources[0]);
    }

    // TODO: change the list to a hashset
    public static CraftTreeNode CreateCraftTree(INodeSource source, List<INodeSource> visitedSources = null, int depth = 0)
    {
        if (depth > MaxDepth)
            throw new Exception("Reached max depth"); // TODO: just return a node saying max depth reached instead...

        var self = new CraftTreeNode(source, [])
        {
            TreeWidth = 1,
            TreeHeight = 1,
        };

        visitedSources ??= [];
        // TODO: does this work with the sources?
        if (visitedSources.Contains(source))
            return self; // TODO: should probably fade duplicated sources

        visitedSources.Add(source);

        // TODO: how to handle crafting stations?
        // TODO: configure options to expand things:
        // - stupid loopds like walls
        var childSources = source.GetChildren().ToArray();
        if (childSources.Length <= 0)
            return self;

        // We have child nodes, so traverse them and add to children, then update the max width
        self.TreeWidth = 0;
        self.TreeHeight = 0;

        foreach (var child in childSources)
        {
            var childNode = CreateCraftTree(child, visitedSources, depth + 1);
            self.Children.Add(childNode);
            self.TreeWidth += childNode.TreeWidth;
            self.TreeHeight = Math.Max(self.TreeHeight, childNode.TreeHeight);
        }

        self.TreeHeight += 1;

        return self;
    }
}
