using InterfacePlus.Utilities;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace InterfacePlus.Content.CraftTree;

// TODO: copy the camera control UI from the unfinished incremental mod
// TODO: give this a border + proper background
public class CraftTreeView : UIElement
{
    public Vector2 Position = Vector2.Zero;
    public float Scale = 1f;
    public CraftTreeNode? CraftTree { get; private set; }

    private static float TranslateSpeed => 3f;
    private static float ScaleSpeed => 1.02f;
    private static int IconVerticalShift => 75;
    private static int IconHorizontalShift => 75;
    private static bool EnableCentering => true;

    private static Asset<Texture2D> IconPanelTexture = Assets.Textures.UI.CraftTreePanel.Asset;

    private readonly List<CraftTreeIcon> treeIcons = [];

    public CraftTreeView()
    {
        OverflowHidden = true;
    }

    public void Reset()
    {
        treeIcons.Clear();
        CraftTree = null;
        ResetTransform();
    }

    public void HandleZoom(bool zoomIn)
    {
        if (zoomIn)
            Scale *= ScaleSpeed;
        else
            Scale /= ScaleSpeed;
    }

    public void HandleTranslate(Vector2 dir)
    {
        Position += TranslateSpeed * dir / Scale;
    }

    public void ResetTransform()
    {
        Scale = 1f;
        Position = Vector2.Zero;
    }

    public void FitTree()
    {
        var positions = treeIcons.Select(icon => icon.Position).ToArray();
        var treeHitbox = Utils.BoundingRectangle(positions);
        treeHitbox.Inflate(IconPanelTexture.Width(), IconPanelTexture.Height());

        var dims = GetDimensions();
        float scaleX = dims.Width / treeHitbox.Width;
        float scaleY = dims.Height / treeHitbox.Height;

        Position = treeHitbox.Center();
        Scale = Math.Min(scaleX, scaleY);
    }

    public void SetItem(int itemType)
    {
        var dimensions = GetDimensions();
        var initialPos = Vector2.UnitY * (-dimensions.Height / 2 + IconVerticalShift);

        CraftTree = CraftTreeTraverser.CreateCraftTree(itemType);
        AddCraftTreeIcon(CraftTree.Value, initialPos);
    }

    private void AddCraftTreeIcon(CraftTreeNode node, Vector2 position, CraftTreeIcon parent = null)
    {
        var icon = new CraftTreeIcon(position, node.ItemType);
        treeIcons.Add(icon);

        var childPosition = position + Vector2.UnitY * IconVerticalShift;

        // TODO: still not correct
        if (EnableCentering)
        {
            if (node.TreeWidth % 2 == 0)
                childPosition.X -= node.TreeWidth / 2f * IconHorizontalShift;
            else
                childPosition.X -= (node.TreeWidth - 1) / 2f * IconHorizontalShift;
        }

        foreach (var child in node.Children)
        {
            AddCraftTreeIcon(child, childPosition, icon);
            childPosition.X += IconHorizontalShift * child.TreeWidth;
        }
    }

    private void DrawCraftTreeIcons(SpriteBatch spriteBatch, Matrix transform)
    {
        foreach (var icon in treeIcons)
        {
            DrawCraftTreeIcon(spriteBatch, transform, icon);
        }
    }

    private void DrawCraftTreeIcon(SpriteBatch spriteBatch, Matrix transform, CraftTreeIcon icon)
    {
        const int CornerSize = 10;
        const int ConnectionLineWidth = 2;
        Color connectionLineColor = new(98, 105, 152);

        var bounds = new Rectangle(
            (int)icon.Position.X - IconPanelTexture.Width() / 2,
            (int)icon.Position.Y - IconPanelTexture.Height() / 2,
            IconPanelTexture.Width(),
            IconPanelTexture.Height()
        );

        var transformedBounds = bounds.Transform(transform);
        bool isMouseHovering = IsMouseHovering && transformedBounds.Contains(Main.MouseScreen.ToPoint());

        // TODO: these lines should be like in HOI4 focus tree
        /* TODO: fix, probably make parent draw all of the connections
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
        */

        Utils.DrawSplicedPanel(
            spriteBatch,
            IconPanelTexture.Value,
            bounds.X,
            bounds.Y,
            bounds.Width,
            bounds.Height,
            CornerSize,
            CornerSize,
            CornerSize,
            CornerSize,
            Color.White
        );

        // TODO: draw item stack text
        // TODO: show crafting requirements
        // TODO: sometimes ItemType is very large?
        if (icon.ItemType != ItemID.None)
        {
            Main.instance.LoadItem(icon.ItemType);
            var texture = TextureAssets.Item[icon.ItemType];

            // TODO: handle item animations
            spriteBatch.Draw(
                texture.Value,
                icon.Position,
                null,
                Color.White,
                0f,
                texture.Size() / 2,
                1f,
                SpriteEffects.None,
                0f
            );
        }

        // TODO: add more info to the tooltip
        if (isMouseHovering)
            UICommon.TooltipMouseText(Lang._itemNameCache[icon.ItemType].Value);
    }

    private Matrix GetMatrix()
    {
        var transform = Matrix.Identity;
        // Camera movement
        transform *= Matrix.CreateTranslation(-Position.X, -Position.Y, 0f);

        // Camera scale
        transform *= Matrix.CreateScale(Scale) * Main.UIScaleMatrix;

        // Shift centre of the UI to (0, 0)
        var center = GetDimensions().Center();
        transform *= Matrix.CreateTranslation(center.X, center.Y, 0f);

        return transform;
    }

    // Modified from UIElement.Draw()
    protected override void DrawChildren(SpriteBatch spriteBatch)
    {
        var matrix = GetMatrix();
        var rasterizerState = spriteBatch.GraphicsDevice.RasterizerState;
        var scissorRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;

        spriteBatch.End();
        spriteBatch.GraphicsDevice.ScissorRectangle = GetClippingRectangle(spriteBatch);
        spriteBatch.GraphicsDevice.RasterizerState = OverflowHiddenRasterizerState;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, OverflowHiddenRasterizerState, null, matrix);

        base.DrawChildren(spriteBatch);
        DrawCraftTreeIcons(spriteBatch, matrix);

        spriteBatch.End();
        spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;
        spriteBatch.GraphicsDevice.RasterizerState = rasterizerState;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, rasterizerState, null, Main.UIScaleMatrix);
    }
}
