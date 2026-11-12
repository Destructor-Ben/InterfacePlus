using InterfacePlus.Utilities;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace InterfacePlus.Content.CraftTree;

// TODO: give this a border + proper background
// TODO: allow exporting to a PNG
// TODO: potentially show a dropdown to select which recipe to show
public class CraftTreeView : UIElement
{
    public Vector2 Position = Vector2.Zero;
    public float Scale = 1f;
    public CraftTreeNode? CraftTree { get; private set; }

    private static float TranslateSpeed => 3f;
    private static float ScaleSpeed => 1.02f;
    private static float ScrollSpeed => 1.2f;
    private static int IconVerticalShift => 75;
    private static int IconHorizontalShift => 75;
    private static bool EnableCentering => true;

    private static readonly Asset<Texture2D> IconPanelTexture = Assets.Textures.UI.CraftTreePanel.Asset;

    private readonly List<CraftTreeIcon> treeIcons = [];
    private bool isMouseDragging = false;

    public CraftTreeView()
    {
        OverflowHidden = true;
    }

    public void Reset()
    {
        treeIcons.Clear();
        CraftTree = null;
        isMouseDragging = false;
        ResetTransform();
    }

    #region Button Actions

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

    #endregion

    #region Craft Tree Icon Creation

    public void SetItem(int itemType)
    {
        var dimensions = GetDimensions();
        var initialPos = Vector2.UnitY * (-dimensions.Height / 2 + IconVerticalShift);

        CraftTree = CraftTreeTraverser.CreateCraftTree(itemType);
        AddCraftTreeIcon(CraftTree.Value, initialPos);
    }

    private CraftTreeIcon AddCraftTreeIcon(CraftTreeNode node, Vector2 position, CraftTreeIcon parent = null)
    {
        var icon = new CraftTreeIcon(position, node.ItemType, parent, []);
        treeIcons.Add(icon);

        var childPosition = position + Vector2.UnitY * IconVerticalShift;

        // TODO: still not correct
        if (EnableCentering)
        {
            if (node.TreeWidth > 1)
                childPosition.X -= node.TreeWidth / 4f * IconHorizontalShift;
        }

        foreach (var childNode in node.Children)
        {
            var childIcon = AddCraftTreeIcon(childNode, childPosition, icon);
            childPosition.X += IconHorizontalShift * childNode.TreeWidth;
            icon.Children.Add(childIcon);
        }

        return icon;
    }

    #endregion

    #region Rendering

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
        float connectionlineHeight = IconVerticalShift / 2f;

        var bounds = new Rectangle(
            (int)icon.Position.X - IconPanelTexture.Width() / 2,
            (int)icon.Position.Y - IconPanelTexture.Height() / 2,
            IconPanelTexture.Width(),
            IconPanelTexture.Height()
        );

        var transformedBounds = bounds.Transform(transform);
        bool isMouseHovering = IsMouseHovering && transformedBounds.Contains(Main.MouseScreen.ToPoint());

        // Draw line up if have parent
        if (icon.Parent is not null)
            DrawConnectionLine(spriteBatch, icon.Position, -connectionlineHeight, true);

        // Parent icons draw the horizontal line connecting all the children and the vertical line connecting parent to the horizontal line
        if (icon.Children.Count > 0)
            DrawConnectionLine(spriteBatch, icon.Position, connectionlineHeight, true);

        if (icon.Children.Count > 1)
        {
            var firstChild = icon.Children[0];
            var lastChild = icon.Children[^1];
            DrawConnectionLine(spriteBatch, firstChild.Position - Vector2.UnitY * connectionlineHeight, lastChild.Position.X - firstChild.Position.X, false);
        }

        // TODO: call into my (to be made) custom itemslot drawing code
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

    private static void DrawConnectionLine(SpriteBatch spriteBatch, Vector2 start, float offset, bool isVertical)
    {
        const float ConnectionLineWidth = 2;
        var connectionLineColor = new Color(98, 105, 152);

        start.X -= ConnectionLineWidth / 2;
        var end = start;

        if (isVertical)
        {
            end.X += ConnectionLineWidth;
            end.Y += offset;
        }
        else
        {
            end.X += offset;
            end.Y += ConnectionLineWidth;
        }

        var targetRectangle = Utils.CornerRectangle(start, end);

        spriteBatch.Draw(
            TextureAssets.MagicPixel.Value,
            targetRectangle,
            null,
            connectionLineColor
        );
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
        var samplerState = Scale >= 1f ? SamplerState.PointClamp : SamplerState.AnisotropicClamp;

        spriteBatch.End();
        spriteBatch.GraphicsDevice.ScissorRectangle = GetClippingRectangle(spriteBatch);
        spriteBatch.GraphicsDevice.RasterizerState = OverflowHiddenRasterizerState;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState, DepthStencilState.None, OverflowHiddenRasterizerState, null, matrix);

        base.DrawChildren(spriteBatch);
        DrawCraftTreeIcons(spriteBatch, matrix);

        spriteBatch.End();
        spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;
        spriteBatch.GraphicsDevice.RasterizerState = rasterizerState;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, rasterizerState, null, Main.UIScaleMatrix);
    }

    #endregion

    #region Input

    public override void ScrollWheel(UIScrollWheelEvent evt)
    {
        base.ScrollWheel(evt);

        float scrollAmount = ScrollSpeed;
        if (evt.ScrollWheelValue < 0)
            scrollAmount = 1 / scrollAmount;

        Scale *= scrollAmount;
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);

        isMouseDragging = true;
    }

    public override void LeftMouseUp(UIMouseEvent evt)
    {
        base.LeftMouseUp(evt);

        isMouseDragging = false;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Stop moving if the mouse has moved out of the ui
        if (!IsMouseHovering) {
            isMouseDragging = false;
        }

        // Mouse movement
        if (isMouseDragging && IsMouseHovering) {
            Position.X -= (Main.mouseX - Main.lastMouseX) / Scale;
            Position.Y -= (Main.mouseY - Main.lastMouseY) / Scale;
        }
    }

    #endregion
}
