using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace InterfacePlus.Content;

// TODO: temporary
public class TestCommand : ModCommand
{
    public override CommandType Type
        => CommandType.Chat;
    public override string Command
        => "craft-tree";

    public override string Description
        => "TEMP";

    public override void SetStaticDefaults() {
    }

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (!int.TryParse(args[0], out int item))
            return;

        CraftTreeUI.OpenForItem(item);
    }
}

public class CraftTreeUI : UIState, ILoadable
{
    public static CraftTreeUI Instance { get; private set; }

    public int? RootItemType = null;

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

    public static void OpenForItem(int itemType)
    {
        Instance.SetRootItem(itemType);
        IngameFancyUI.OpenUIState(Instance);
    }

    public void SetRootItem(int itemType)
    {
        RootItemType = itemType;
    }

    // TODO: copy the camera control UI from the unfinished incremental mod
    // TODO: Make a crafting indicator like the one used in NewCraftingUI showing the ingredients and result
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
        // TODO: go back to the recipe browser
        IngameFancyUI.Close();
    }
}
