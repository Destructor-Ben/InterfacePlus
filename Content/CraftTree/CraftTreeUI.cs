using InterfacePlus.Utilities;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace InterfacePlus.Content.CraftTree;

public class CraftTreeUI : UIState, ILoadable
{
    public static CraftTreeUI Instance { get; private set; }

    public int RootItemType = ItemID.None;

    private NewCraftingUI.RecipeFilter craftingFilter;
    private string searchFilter;
    private int? selectedRecipe;
    private bool wasInGuideCraftMenu;
    private int talkNpc;

    private UIElement rootElement;
    private UIPanel rootPanel;
    private UITextPanel<object> headerPanel;
    private UITextPanel<object> backButton;
    private CraftTreeView craftTree;
    private UIElement topBar;

    private Asset<Texture2D> craftIcon;
    private Asset<Texture2D> stationsIcon;

    public void Load(Mod mod)
    {
        Instance = this;
    }

    public void Unload() { }

    public override void OnActivate()
    {
        craftIcon = Main.Assets.Request<Texture2D>("Images/UI/Craft");
        stationsIcon = Assets.Textures.UI.CraftingStations.Asset;

        RemoveAllChildren();
        CreateUI();
        SetRootItem(RootItemType);
    }

    public static void OpenForItem(int itemType, bool quiet = false)
    {
        Instance.craftingFilter = NewCraftingUI._instance._filter;
        Instance.searchFilter = NewCraftingUI._instance._searchBar._searchString;
        Instance.selectedRecipe = NewCraftingUI._instance._selectedRecipeIndex;
        Instance.wasInGuideCraftMenu = Main.InGuideCraftMenu;
        Instance.talkNpc = Main.LocalPlayer.talkNPC;

        Instance.SetRootItem(itemType);
        IngameFancyUI.OpenUIState(Instance);

        if (!quiet)
            SoundEngine.PlaySound(SoundID.MenuOpen);
    }

    public void SetRootItem(int itemType)
    {
        RootItemType = itemType;

        if (RootItemType != ItemID.None && craftTree is not null)
            craftTree.SetItem(RootItemType);
    }

    private void CreateUI()
    {
        rootElement = new UIElement
        {
            Width = { Percent = 0.6f },
            Top = { Pixels = 220 },
            Height = { Pixels = -200, Percent = 1f },
            HAlign = 0.5f,
        };
        Append(rootElement);

        rootPanel = new UIPanel
        {
            Width = { Percent = 1f },
            Height = { Percent = 1f, Pixels = -110 },
            BackgroundColor = UICommon.MainPanelBackground,
        };
        rootElement.Append(rootPanel);

        headerPanel = new UITextPanel<object>(ModInstance.GetLocalization("UI.CraftTree.DisplayName").Value, 0.7f, large: true)
        {
            Top = { Pixels = -45 },
            HAlign = 0.5f,
            BackgroundColor = UICommon.DefaultUIBlue,
        }.WithPadding(15);
        rootElement.Append(headerPanel);

        backButton = new UITextPanel<object>(Language.GetText("UI.Back"), 0.7f, large: true)
        {
            Width = { Percent = 0.5f, Pixels = -10 },
            Height = { Pixels = 50 },
            Top = { Pixels = -45 },
            VAlign = 1f,
            HAlign = 0.5f,
        }.WithFadedMouseOver();
        backButton.OnLeftClick += (_, _) => GoBack();
        rootElement.Append(backButton);

        topBar = new UIElement
        {
            Width = { Percent = 1f },
            Height = { Pixels = TextureAssets.InventoryBack.Height() },
        };
        rootPanel.Append(topBar);

        craftTree = new CraftTreeView
        {
            Width = { Percent = 1f },
            Height = { Percent = 1f, Pixels = -(topBar.Height.Pixels + rootPanel.PaddingTop) },
            VAlign = 1f,
        };
        rootPanel.Append(craftTree);

        CreateTopBarUI();
    }

    private void CreateTopBarUI()
    {
        const float Padding = 5f;
        float left = 0;

        CreateButton(Assets.Textures.UI.TransformButton_ZoomIn.Asset,  () => craftTree.HandleZoom(true));
        CreateButton(Assets.Textures.UI.TransformButton_ZoomOut.Asset, () => craftTree.HandleZoom(false));
        left -= Padding;

        CreateButton(Assets.Textures.UI.TransformButton_MoveDown.Asset,  () => craftTree.HandleTranslate(Vector2.UnitY));
        CreateButton(Assets.Textures.UI.TransformButton_MoveUp.Asset,    () => craftTree.HandleTranslate(-Vector2.UnitY));
        CreateButton(Assets.Textures.UI.TransformButton_MoveRight.Asset, () => craftTree.HandleTranslate(Vector2.UnitX));
        CreateButton(Assets.Textures.UI.TransformButton_MoveLeft.Asset,  () => craftTree.HandleTranslate(-Vector2.UnitX));
        left -= Padding;

        CreateButton(Assets.Textures.UI.TransformButton_FitTree.Asset, () => craftTree.FitTree());
        CreateButton(Assets.Textures.UI.TransformButton_Reset.Asset,  () => craftTree.ResetTransform());

        return;

        void CreateButton(Asset<Texture2D> icon, Action runWhileClicked)
        {
            var button = new UIImageButton(icon.WaitForMe())
            {
                VAlign = 0.5f,
                HAlign = 1f,
                Left = { Pixels = left },
            };

            button.SetVisibility(1f, 1f);
            button.SetHoverImage(Assets.Textures.UI.TransformButton_Hover.Asset.WaitForMe());
            button.OnUpdate += (element) =>
            {
                if (element.IsMouseHovering && Main.mouseLeft)
                    runWhileClicked();
            };

            topBar.Append(button);
            left -= button.Width.Pixels + Padding;
        }
    }

    private void GoBack()
    {
        IngameFancyUI.Close();

        Main.InGuideCraftMenu = wasInGuideCraftMenu;
        if (Main.InGuideCraftMenu)
        {
            // Modified from Main.NPCChatText_GuideReverseCrafting
            Main.playerInventory = true;
            Main.npcChatText = "";
            Main.LocalPlayer.SetTalkNPC(talkNpc);
            NewCraftingUI.Open(quiet: true);

            Main.TryChangePipsPage(Main.PipPage.Recipes);
            UILinkPointNavigator.GoToDefaultPage();
        }
        else
        {
            Player.OpenInventory(quiet: true);
            NewCraftingUI.Open(quiet: true, craftingFilter);
            NewCraftingUI._instance._searchBar.SetContents(searchFilter);
            NewCraftingUI._instance._selectedRecipeIndex = selectedRecipe;
        }

        craftingFilter = null;
        searchFilter = "";
        selectedRecipe = null;
        wasInGuideCraftMenu = false;
        talkNpc = -1;
    }

    protected override void DrawChildren(SpriteBatch spriteBatch)
    {
        base.DrawChildren(spriteBatch);
        DrawRecipe(spriteBatch);
    }

    // Modified from NewCraftingUI.DrawChildren()
    // TODO: i think the mouse layer doesn't draw?
    private void DrawRecipe(SpriteBatch spriteBatch)
    {
        if (RootItemType == ItemID.None)
            return;

        // TODO: temp
        // spriteBatch.Draw(TextureAssets.MagicPixel.Value, topBar.GetDimensions().ToRectangle(), null, Color.Red);

        // TODO: how to handle multiple recipes?
        var recipe = Main.recipe.FirstOrDefault(r => r.createItem.type == RootItemType);
        if (recipe == null)
            return;

        // Craft slot
        var pos = topBar.GetInnerDimensions().Position();
        if (DrawRecipeSlot(spriteBatch, recipe.createItem, pos, ItemSlot.Context.NewCraftingUICraftSlot, out var bounds))
            ItemSlot.MouseHover(recipe.createItem, ItemSlot.Context.NewCraftingUICraftSlot);

        pos.X += bounds.Width;

        // Craft icon
        spriteBatch.Draw(
            craftIcon.Value,
            pos + bounds.Size() / 2f,
            null,
            Color.White,
            0f,
            craftIcon.Size() / 2f,
            1f,
            SpriteEffects.None,
            0f
        );

        pos.X += bounds.Width;

        // Ingredients
        int ingredientCount = recipe.requiredItem.Count;
        float ingredientScale = Math.Min(11f / (float)ingredientCount, 1f);
        for (int j = 0; j < ingredientCount; j++)
        {
            Item item = recipe.requiredItem[j];
            UILinkPointNavigator.Shortcuts.NewCraftingUI_MaterialIndex = j;
            if (DrawRecipeSlot(spriteBatch, item, pos, ItemSlot.Context.NewCraftingUIMaterial, out var _, 1f * ingredientScale))
            {
                // TODO:
                ItemSlot.MouseHover(item, ItemSlot.Context.NewCraftingUIMaterial);
                // CraftingUI.SetRecipeMaterialDisplayName(recipe, item);
            }

            pos.X += bounds.Width * ingredientScale;
        }

        // Stations icon
        // TODO: run the code below this first (and add the data to a list) then check if the list is empty, and if it isn't, then draw the icons
        if (recipe.requiredTile < 0
            && !recipe.needWater
            && !recipe.needHoney
            && !recipe.needLava
            && !recipe.needSnowBiome
            && !recipe.needGraveyardBiome
            && !recipe.needTorchGodsFavor
            && !recipe.needMechdusa)
            return;

        spriteBatch.Draw(
            stationsIcon.Value,
            pos + bounds.Size() / 2f,
            null,
            Color.White,
            0f,
            stationsIcon.Size() / 2f,
            1f,
            SpriteEffects.None,
            0f
        );

        pos.X += bounds.Width;

        // Crafting stations
        if (recipe.requiredTile >= 0) {
            DrawIngredientSlot(spriteBatch, ItemID.None, ref pos);
            // TODO: use fake Air item, just draw the crafting station in the slot
        }

        if (recipe.needWater)
            DrawIngredientSlot(spriteBatch, ItemID.WaterBucket, ref pos);

        if (recipe.needHoney)
            DrawIngredientSlot(spriteBatch, ItemID.HoneyBucket, ref pos);

        if (recipe.needLava)
            DrawIngredientSlot(spriteBatch, ItemID.LavaBucket, ref pos);

        if (recipe.needSnowBiome)
            DrawIngredientSlot(spriteBatch, ItemID.SnowBlock, ref pos);

        if (recipe.needGraveyardBiome)
            DrawIngredientSlot(spriteBatch, ItemID.Tombstone, ref pos);

        if (recipe.needTorchGodsFavor)
            DrawIngredientSlot(spriteBatch, ItemID.TorchGodsFavor, ref pos);

        if (recipe.needMechdusa)
            DrawIngredientSlot(spriteBatch, ItemID.MechdusaSummon, ref pos);

        // TODO: handle other conditions that mods can add
    }

    private static void DrawIngredientSlot(SpriteBatch spriteBatch, int itemType, ref Vector2 pos)
    {
        var item = new Item(itemType);
        DrawIngredientSlot(spriteBatch, item, ref pos);
    }

    private static void DrawIngredientSlot(SpriteBatch spriteBatch, Item item, ref Vector2 pos)
    {
        if (DrawRecipeSlot(spriteBatch, item, pos, ItemSlot.Context.NewCraftingUIMaterial, out var bounds))
            ItemSlot.MouseHover(item, ItemSlot.Context.NewCraftingUIMaterial);

        pos.X += bounds.Width;
    }

    // TODO: see if all the contexts are good and if any of them should change
    // Returns true if the mouse is hovering
    private static bool DrawRecipeSlot(SpriteBatch spriteBatch, Item item, Vector2 pos, int context, out Rectangle bounds, float scale = 1f)
    {
        var inventoryBack = Main.inventoryBack;
        Main.inventoryBack = Color.White * 0.7490196f;
        float inventoryScale = Main.inventoryScale;
        Main.inventoryScale = scale;

        ItemSlot.Draw(spriteBatch, ref item, context, pos, Color.White);

        Main.inventoryScale = inventoryScale;
        Main.inventoryBack = inventoryBack;

        bounds = new Rectangle(
            (int)pos.X,
            (int)pos.Y,
            (int)((float)TextureAssets.InventoryBack.Width() * scale),
            (int)((float)TextureAssets.InventoryBack.Height() * scale)
        );

        return bounds.Contains(Main.MouseScreen.ToPoint());
    }
}
