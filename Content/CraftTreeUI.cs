using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace InterfacePlus.Content;

public class CraftTreeUI : UIState, ILoadable
{
    public static CraftTreeUI Instance { get; private set; }

    public int? RootItemType = null;

    private NewCraftingUI.RecipeFilter craftingUiFilter;
    private bool wasInGuideCraftMenu;
    private int talkNpc;

    private UIElement rootElement;
    private UIPanel rootPanel;
    private UITextPanel<object> headerPanel;
    private UITextPanel<object> backButton;

    public void Load(Mod mod)
    {
        Instance = this;
    }

    public void Unload() { }

    public override void OnActivate()
    {
        RemoveAllChildren();
        CreateUI();
    }

    public static void OpenForItem(int itemType, bool quiet = false)
    {
        Instance.craftingUiFilter = NewCraftingUI._instance._filter;
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
    }

    // TODO: bigger main panel
    // TODO: copy the camera control UI from the unfinished incremental mod
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
            NewCraftingUI.Open(quiet: true, craftingUiFilter);
        }

        craftingUiFilter = null;
        wasInGuideCraftMenu = false;
        talkNpc = -1;
    }
}
