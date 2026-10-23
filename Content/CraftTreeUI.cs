using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace InterfacePlus.Content;

public class CraftTreeUI : UIState, ILoadable
{
    public static CraftTreeUI Instance { get; private set; }

    // TODO: just use ItemID.None
    public int? RootItemType = null;

    private NewCraftingUI.RecipeFilter craftingFilter;
    private bool wasInGuideCraftMenu;
    private int talkNpc;

    private UIElement rootElement;
    private UIPanel rootPanel;
    private UITextPanel<object> headerPanel;
    private UITextPanel<object> backButton;
    private CraftTreeView craftTree;

    public void Load(Mod mod)
    {
        Instance = this;
    }

    public void Unload() { }

    public override void OnActivate()
    {
        RemoveAllChildren();
        CreateUI();
        SetRootItem(RootItemType);
    }

    public static void OpenForItem(int itemType, bool quiet = false)
    {
        Instance.craftingFilter = NewCraftingUI._instance._filter;
        Instance.wasInGuideCraftMenu = Main.InGuideCraftMenu;
        Instance.talkNpc = Main.LocalPlayer.talkNPC;

        Instance.SetRootItem(itemType);
        IngameFancyUI.OpenUIState(Instance);

        if (!quiet)
            SoundEngine.PlaySound(SoundID.MenuOpen);
    }

    public void SetRootItem(int? itemType)
    {
        RootItemType = itemType;

        if (RootItemType is not null && craftTree is not null)
            craftTree.SetItem(RootItemType.Value);
    }

    // TODO: bigger main panel
    // TODO: Make a crafting indicator like the one used in NewCraftingUI showing the ingredients and result and crafting bench
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

        craftTree = new CraftTreeView
        {
            Width = { Percent = 1f },
            Height = { Percent = 1f },
        };
        rootPanel.Append(craftTree);
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
        }

        craftingFilter = null;
        wasInGuideCraftMenu = false;
        talkNpc = -1;
    }
}

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
        // TODO: also display item drop data and NPC drop data and tile drop data and where tiles spawn
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

public class CraftTreeIcon : UIElement
{
    public int ItemType = ItemID.None;
    public CraftTreeIcon? ParentIcon = null;

    private Asset<Texture2D> panelTexture = ModInstance.Assets.Request<Texture2D>("Assets/Textures/UI/CraftTreePanel");
    private static int CornerSize => 10;
    private static int ConnectionLineWidth => 2;
    private static Color ConnectionLineColor => new(98, 105, 152);

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var dimensions = GetDimensions();

        // TODO: these lines should be like in HOI4 focus tree
        if (ParentIcon is not null)
        {
            int heightDifference = (int)ParentIcon.GetDimensions().Y - (int)dimensions.Y;

            var targetRectangle = new Rectangle(
                (int)dimensions.X - ConnectionLineWidth / 2,
                (int)dimensions.Y - heightDifference / 2,
                ConnectionLineWidth,
                heightDifference / 2
            );

            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                targetRectangle,
                null,
                ConnectionLineColor
            );
        }

        Utils.DrawSplicedPanel(
            spriteBatch,
            panelTexture.Value,
            (int)dimensions.X - panelTexture.Width() / 2,
            (int)dimensions.Y - panelTexture.Height() / 2,
            panelTexture.Width(),
            panelTexture.Height(),
            CornerSize,
            CornerSize,
            CornerSize,
            CornerSize,
            Color.White
        );

        // TODO: draw item
        if (ItemType != ItemID.None)
        {
            Main.instance.LoadItem(ItemType);
            var texture = TextureAssets.Item[ItemType];

            // TODO: handle item animations
            spriteBatch.Draw(
                texture.Value,
                dimensions.Center(),
                null,
                Color.White,
                0f,
                texture.Size() / 2,
                1f,
                SpriteEffects.None,
                0f
            );
        }
    }
}
