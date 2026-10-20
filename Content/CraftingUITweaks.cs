using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using RecipeEntry = Terraria.GameContent.UI.NewCraftingUI.RecipeEntry;

namespace InterfacePlus.Content;

public class CraftingUITweaks : ILoadable
{
    private bool showAllRecipes = false;
    private bool ShowAllRecipes => !Main.InGuideCraftMenu && showAllRecipes;

    private UIImageButton toggleAllRecipesButton;

    public void Load(Mod mod)
    {
        On_NewCraftingUI.BuildInfinitesMenuContents += (orig, self, container) =>
        {
            orig(self, container);
            AddShowAllRecipesToggle(self);
            AddOpenCraftTreeButton(self);
        };

        // Perhaps IL edit in future
        On_NewCraftingUI.UpdateText += (orig, self) =>
        {
            string text = self._filter != null ? self._filter.GetWindowDescription() : Lang.inter[25].Value;
            bool shouldUpdate = text != self._text.Text;
            orig(self);

            if (shouldUpdate)
                UpdateSearchBarFromAllRecipesToggle(self);
        };

        // TODO: do this via an IL edit in future
        On_NewCraftingUI.UpdateContents += (orig, self) =>
        {
            // orig(self);
            UpdateContents(self);
        };
    }

    public void Unload() { }

    private UIPanel GetUIPanel(NewCraftingUI self)
    {
        var uiPanel = (UIPanel)self._text.Parent;
        if (uiPanel is null)
            throw new Exception("Unable to get the root UIPanel for the crafting UI");

        return uiPanel;
    }

    // TODO: disable this toggle while in the guide menu
    private void AddShowAllRecipesToggle(NewCraftingUI self)
    {
        var uiPanel = GetUIPanel(self);

        var backgroundImageEnabled = ModInstance.Assets.Request<Texture2D>("Assets/Textures/UI/ToggleAllRecipesButton_Enabled", AssetRequestMode.ImmediateLoad);
        var backgroundImageDisabled = ModInstance.Assets.Request<Texture2D>("Assets/Textures/UI/ToggleAllRecipesButton_Disabled", AssetRequestMode.ImmediateLoad);
        var hoverImage = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Button_Search_Border", AssetRequestMode.ImmediateLoad);

        toggleAllRecipesButton = new UIImageButton(showAllRecipes ? backgroundImageEnabled : backgroundImageDisabled) {
            HAlign = 1f,
            Top = { Pixels = -1 },
        };
        toggleAllRecipesButton.SetHoverImage(hoverImage);
        toggleAllRecipesButton.SetVisibility(1f, 1f);

        toggleAllRecipesButton.OnLeftClick += (_, _) => {
            showAllRecipes = !showAllRecipes;
            toggleAllRecipesButton.SetImage(showAllRecipes ? backgroundImageEnabled : backgroundImageDisabled);
            SoundEngine.PlaySound(SoundID.MenuTick);
            self.ResetRecipes();
        };

        toggleAllRecipesButton.OnDraw += (_, _) =>
        {
            if (!toggleAllRecipesButton.IsMouseHovering)
                return;

            UICommon.TooltipMouseText(ModInstance.GetLocalization("UI.Crafting.ToggleAllRecipes").Value);
        };

        uiPanel.Append(toggleAllRecipesButton);

        UpdateSearchBarFromAllRecipesToggle(self);
    }

    private void UpdateSearchBarFromAllRecipesToggle(NewCraftingUI self)
    {
        const float SearchBarPadding = 5;
        var searchBar = self._searchBar;
        float searchBarShift = SearchBarPadding + toggleAllRecipesButton.GetDimensions().Width;
        searchBar.Left.Pixels = -searchBarShift;
        searchBar.Width.Pixels -= searchBarShift;
        searchBar.Recalculate();
    }

    // TODO: disable this toggle while in the guide menu
    // TODO: disable (temporarily) if there is no selected entry
    // TODO: either shrink the ingredients list or move this
    private void AddOpenCraftTreeButton(NewCraftingUI self)
    {
        var uiPanel = GetUIPanel(self);

        var texture = Main.Assets.Request<Texture2D>("Images/UI/ButtonBacking", AssetRequestMode.ImmediateLoad);
        var button = new UIImageButton(texture)
        {
            Top = { Pixels = 50 },
            HAlign = 1f,
        };
        button.OnLeftClick += (_, _) =>
        {
            if (self.SelectedEntry is null)
                return;

            CraftTreeUI.OpenForItem(self.SelectedEntry.Recipe.createItem.type);
        };
        uiPanel.Append(button);
    }

    private void UpdateContents(NewCraftingUI self)
    {
        Recipe.UpdateRecipeList();
        if (Main.InGuideCraftMenu && Main.guideItem != self._resetForGuideItem)
            self.ResetRecipes();

        if (ShowAllRecipes)
            self.ResetRecipes();

        foreach (var recipe in self._recipes) {
            recipe.availableIndex = -1;
        }

        int numRecipes = self.GetNumRecipes();
        bool flag = self._filteredRecipes.Count == 0;
        for (int i = 0; i < numRecipes; i++) {
            int num = Main.availableRecipe[i];
            RecipeEntry recipeEntry = self._recipeListLookup[num];
            if (recipeEntry == null) {
                recipeEntry = (self._recipeListLookup[num] = new RecipeEntry(num));
                self._recipes.Add(recipeEntry);
                if (self.FitsFilter(recipeEntry.Recipe)) {
                    recipeEntry.gridIndex = self._filteredRecipes.Count;
                    self._filteredRecipes.Add(recipeEntry);
                }
    	    }

		    recipeEntry.availableIndex = i;
	    }

	    if (ShowAllRecipes)
	    {
		    for (int i = 0; i < Recipe.numRecipes; i++) {
			    RecipeEntry recipeEntry = self._recipeListLookup[i];
			    if (recipeEntry == null) {
				    recipeEntry = (self._recipeListLookup[i] = new RecipeEntry(i));
				    self._recipes.Add(recipeEntry);
				    if (self.FitsFilter(recipeEntry.Recipe)) {
					    recipeEntry.gridIndex = self._filteredRecipes.Count;
					    self._filteredRecipes.Add(recipeEntry);
				    }
			    }
		    }
	    }

	    if (self.SelectedEntry == null)
		    self._selectedRecipeIndex = null;
	    else if (self._filter != null && !self._filter.Accepts(self.SelectedEntry.Recipe))
		    self._selectedRecipeIndex = null;
	    else if (self.SelectedEntry.Available)
		    Main.focusRecipe = self.SelectedEntry.availableIndex;

	    if (self._itemGrid.Count != self._filteredRecipes.Count || (flag && self._filteredRecipes.Count > 0) || ShowAllRecipes)
		    self._itemGrid.SetContentsToShow(self._filteredRecipes);
    }
}
