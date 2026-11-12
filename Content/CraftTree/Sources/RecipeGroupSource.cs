namespace InterfacePlus.Content.CraftTree.Sources;

public record RecipeGroupSource(RecipeGroup Group, int Stack = 1) : INodeSource
{
    // TODO: cycle through them
    public Item DrawnItem => new(Group.ValidItems.First(), Stack);

    public IEnumerable<INodeSource> GetChildren()
    {
        // TODO: how to handle multiple ways to craft a recipe group?
        // var sources = new List<INodeSource>();
        //
        // foreach (int ingredientType in Group.ValidItems)
        // {
        //     sources.AddRange(CraftTreeTraverser.GetSourcesForItem(ingredientType, Stack));
        // }
        //
        // return sources;

        // TODO: temp
        return [CraftTreeTraverser.GetSourcesForItem(Group.ValidItems.First(), Stack).First()];
    }
}
