using Terraria.UI;

namespace InterfacePlus.Utilities;

public static class ItemSlotUtils
{
    public static readonly Asset<Texture2D> PanelTexture = Assets.Textures.UI.CraftTreePanel.Asset;

    public static Rectangle DrawCraftTreeSlot(SpriteBatch spriteBatch, Item item, Vector2 pos)
    {
        spriteBatch.Draw(
            PanelTexture.Value,
            pos,
            null,
            Color.White
        );

        return DrawItemInSlot(spriteBatch, item, pos);
    }

    // We do this instead of calling ItemSlot.DrawItemInSlot because we also want the vanilla style stack text
    public static Rectangle DrawItemInSlot(SpriteBatch spriteBatch, Item item, Vector2 pos)
    {
        var inventoryBack = Main.inventoryBack;
        Main.inventoryBack = Color.Transparent;
        float inventoryScale = Main.inventoryScale;
        Main.inventoryScale = 1f;

        ItemSlot.Draw(spriteBatch, ref item, ItemSlot.Context.InWorldDisplay, pos, Color.White);

        Main.inventoryScale = inventoryScale;
        Main.inventoryBack = inventoryBack;

        var bounds = new Rectangle(
            (int)pos.X,
            (int)pos.Y,
            (int)(PanelTexture.Width()),
            (int)(PanelTexture.Height())
        );

        return bounds;
    }
}
