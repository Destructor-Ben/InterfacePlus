namespace InterfacePlus.Content.CraftTree.Sources;

public record RecipeSource(Recipe Recipe) : INodeSource
{
    public Item DrawnItem => Recipe.createItem;

    public IEnumerable<INodeSource> GetChildren()
    {
        var sources = new List<INodeSource>();

        foreach (var ingredient in Recipe.requiredItemQuickLookup)
        {
            if (ingredient.IsRecipeGroup)
            {
                sources.Add(new RecipeGroupSource(ingredient.RecipeGroup, ingredient.stack));
                continue;
            }

            // TODO: how to handle multiple recipes for a single ingredient?
            // sources.AddRange(CraftTreeTraverser.GetSourcesForItem(ingredient.itemIdOrRecipeGroup, ingredient.stack));

            // TODO: temp
            sources.Add(CraftTreeTraverser.GetSourcesForItem(ingredient.itemIdOrRecipeGroup, ingredient.stack).First());
        }

        return sources;
    }
}
