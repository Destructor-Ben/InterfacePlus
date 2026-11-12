using InterfacePlus.Utilities;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Gamepad;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace InterfacePlus.Content.CraftTree;

// TODO: fix first opening being weirdly offset
// - probably because topbar height is set based on a texture that doesn't load immediately
public class CraftTreeUI : UIState, ILoadable, IHaveBackButtonCommand
{
    public static CraftTreeUI Instance { get; private set; }

    public int RootItemType = ItemID.None;

    public UIState PreviousUIState { get; set; } // Unused

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

    private static float Padding => 5f;

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
            Height = { Pixels = ItemSlotUtils.PanelTexture.Height() },
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
        float left = 0;

        CreateButton(Assets.Textures.UI.TransformButton_ZoomIn.Asset,  () => craftTree.HandleZoom(true));
        CreateButton(Assets.Textures.UI.TransformButton_ZoomOut.Asset, () => craftTree.HandleZoom(false));
        left -= Padding;

        CreateButton(Assets.Textures.UI.TransformButton_MoveDown.Asset,  () => craftTree.HandleTranslate(Vector2.UnitY));
        CreateButton(Assets.Textures.UI.TransformButton_MoveUp.Asset,    () => craftTree.HandleTranslate(-Vector2.UnitY));
        CreateButton(Assets.Textures.UI.TransformButton_MoveRight.Asset, () => craftTree.HandleTranslate(Vector2.UnitX));
        CreateButton(Assets.Textures.UI.TransformButton_MoveLeft.Asset,  () => craftTree.HandleTranslate(-Vector2.UnitX));
        left -= Padding;

        CreateButton(Assets.Textures.UI.TransformButton_FitTree.Asset, () => craftTree.FitTree(),        true, ModInstance.GetLocalization("UI.CraftTree.FitTree"));
        CreateButton(Assets.Textures.UI.TransformButton_Reset.Asset,   () => craftTree.ResetTransform(), true, ModInstance.GetLocalization("UI.CraftTree.ResetTransform"));

        return;

        void CreateButton(Asset<Texture2D> icon, Action action, bool singleClickEffect = false, LocalizedText tooltip = null)
        {
            var button = new UIImageButton(icon.WaitForMe())
            {
                VAlign = 0.5f,
                HAlign = 1f,
                Left = { Pixels = left },
            };

            button.SetVisibility(1f, 1f);
            button.SetHoverImage(Assets.Textures.UI.TransformButton_Hover.Asset.WaitForMe());

            if (tooltip is not null)
            {
                button.OnDraw += (element, _) =>
                {
                    if (element.IsMouseHovering)
                        UICommon.TooltipMouseText(tooltip.Value);
                };
            }

            if (singleClickEffect)
            {
                button.OnLeftClick += (_, _) =>
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    action();
                };
            }
            else
            {
                button.OnUpdate += (element) =>
                {
                    if (element.IsMouseHovering && Main.mouseLeft)
                        action();
                };
            }

            topBar.Append(button);
            left -= button.Width.Pixels + Padding;
        }
    }

    public void HandleBackButtonUsage()
    {
        GoBack();
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
    // TODO: how to handle overflow?
    private void DrawRecipe(SpriteBatch spriteBatch)
    {
        if (RootItemType == ItemID.None)
            return;

        // TODO: how to handle multiple recipes?
        var recipe = Main.recipe.FirstOrDefault(r => r.createItem.type == RootItemType);
        if (recipe == null)
            return;

        // Craft slot
        var pos = topBar.GetInnerDimensions().Position();
        DrawSlot(spriteBatch, recipe.createItem, ref pos);

        // Craft icon
        spriteBatch.Draw(
            craftIcon.Value,
            pos + Vector2.UnitY * ItemSlotUtils.PanelTexture.Height() / 2f,
            null,
            Color.White,
            0f,
            Vector2.UnitY * craftIcon.Height() / 2f,
            1f,
            SpriteEffects.None,
            0f
        );

        pos.X += craftIcon.Width() + Padding;

        // Ingredients
        foreach (var ingredient in recipe.requiredItem)
        {
            DrawSlot(spriteBatch, ingredient, ref pos);
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
            pos + Vector2.UnitY * ItemSlotUtils.PanelTexture.Height() / 2f,
            null,
            Color.White,
            0f,
            Vector2.UnitY * stationsIcon.Height() / 2f,
            1f,
            SpriteEffects.None,
            0f
        );

        pos.X += stationsIcon.Width() + Padding;

        // Crafting stations
        if (recipe.requiredTile >= 0) {
            DrawSlot(spriteBatch, ItemID.None, ref pos);
            // TODO: use fake Air item, just draw the crafting station in the slot
        }

        if (recipe.needWater)
            DrawSlot(spriteBatch, ItemID.WaterBucket, ref pos);

        if (recipe.needHoney)
            DrawSlot(spriteBatch, ItemID.HoneyBucket, ref pos);

        if (recipe.needLava)
            DrawSlot(spriteBatch, ItemID.LavaBucket, ref pos);

        if (recipe.needSnowBiome)
            DrawSlot(spriteBatch, ItemID.SnowBlock, ref pos);

        if (recipe.needGraveyardBiome)
            DrawSlot(spriteBatch, ItemID.Tombstone, ref pos);

        if (recipe.needTorchGodsFavor)
            DrawSlot(spriteBatch, ItemID.TorchGodsFavor, ref pos);

        if (recipe.needMechdusa)
            DrawSlot(spriteBatch, ItemID.MechdusaSummon, ref pos);

        // TODO: handle other conditions that mods can add
    }

    private static void DrawSlot(SpriteBatch spriteBatch, int itemType, ref Vector2 pos)
    {
        var item = new Item(itemType);
        DrawSlot(spriteBatch, item, ref pos);
    }

    private static void DrawSlot(SpriteBatch spriteBatch, Item item, ref Vector2 pos)
    {
        var bounds = ItemSlotUtils.DrawCraftTreeSlot(spriteBatch, item, pos);
        pos.X += bounds.Width + Padding;

        if (!bounds.Contains(Main.MouseScreen.ToPoint()))
            return;

        Main.instance.MouseText("");
        Main.HoverItem = item;
    }
}
