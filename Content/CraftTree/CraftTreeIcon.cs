using Terraria.ModLoader.UI;
using Terraria.UI;

namespace InterfacePlus.Content.CraftTree;

public class CraftTreeIcon : UIElement
{
    public int ItemType = ItemID.None;
    public CraftTreeIcon ParentIcon = null;

    private readonly Asset<Texture2D> panelTexture = Assets.Textures.UI.CraftTreePanel.Asset;
    private static int CornerSize => 10;
    private static int ConnectionLineWidth => 2;
    private static Color ConnectionLineColor => new(98, 105, 152);

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var dimensions = GetDimensions();

        // TODO: these lines should be like in HOI4 focus tree
        if (ParentIcon is not null)
        {
            // TODO: fix this
            int heightDifference = (int)ParentIcon.GetDimensions().Y - (int)dimensions.Y;

            var targetRectangle = new Rectangle(
                (int)dimensions.X - ConnectionLineWidth / 2,
                (int)dimensions.Y + heightDifference / 8,
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

        // TODO: draw item stack text
        // TODO: show crafting requirements
        // TODO: sometimes ItemType is very large?
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

        // TODO: add more info to the tooltip (and also it should probably have the transform matrix applied to IsMouseHovering?)
        if (IsMouseHovering)
        {
            UICommon.TooltipMouseText(Lang._itemNameCache[ItemType].Value);
        }
    }
}
