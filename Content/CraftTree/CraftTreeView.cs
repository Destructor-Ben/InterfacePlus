using InterfacePlus.Utilities;
using Terraria.UI;

namespace InterfacePlus.Content.CraftTree;

// TODO: give this a border + proper background
// TODO: allow exporting to a PNG
// TODO: potentially show a dropdown to select which recipe to show
public class CraftTreeView : UIElement
{
    public Vector2 Position = Vector2.Zero;
    public float Scale = 1f;
    public CraftTreeNode CraftTree { get; private set; }

    private static float TranslateSpeed => 3f;
    private static float ScaleSpeed => 1.02f;
    private static float ScrollSpeed => 1.2f;
    private static int IconVerticalShift => 75;
    private static int IconHorizontalShift => 75;
    private static bool EnableCentering => true;

    private readonly List<CraftTreeIcon> treeIcons = [];
    private bool isMouseDragging = false;

    public CraftTreeView()
    {
        OverflowHidden = true;
    }

    public void SetItem(int itemType)
    {
        Reset();
        // TODO: correctly specify stack (probably based recipe)
        CraftTree = CraftTreeTraverser.CreateCraftTree(itemType);
    }

    public void Reset()
    {
        CraftTree = null;
        Recalculate();
    }

    public override void Recalculate()
    {
        treeIcons.Clear();
        isMouseDragging = false;
        ResetTransform();

        base.Recalculate();

        // Done in recalculate because GetDimensions() is wrong in SetItem
        AddCraftTreeIcons();
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
        treeHitbox.Inflate(ItemSlotUtils.PanelTexture.Width(), ItemSlotUtils.PanelTexture.Height());

        var dims = GetDimensions();
        float scaleX = dims.Width / treeHitbox.Width;
        float scaleY = dims.Height / treeHitbox.Height;

        Position = treeHitbox.Center();
        Scale = Math.Min(scaleX, scaleY);
    }

    #endregion

    #region Craft Tree Icon Creation

    private void AddCraftTreeIcons()
    {
        if (CraftTree is null)
            return;

        var dimensions = GetDimensions();
        var initialPos = Vector2.UnitY * (-dimensions.Height / 2 + IconVerticalShift);
        AddCraftTreeIcon(CraftTree, initialPos);
    }

    private CraftTreeIcon AddCraftTreeIcon(CraftTreeNode node, Vector2 position, CraftTreeIcon parent = null)
    {
        var icon = new CraftTreeIcon(node, position, parent, []);
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
        float connectionlineHeight = IconVerticalShift / 2f;

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

        var drawnItem = icon.Node.Source.DrawnItem;
        var bounds = ItemSlotUtils.DrawCraftTreeSlot(spriteBatch, drawnItem, icon.Position - ItemSlotUtils.PanelTexture.Size() / 2f);

        var transformedBounds = bounds.Transform(transform);
        bool isMouseHovering = IsMouseHovering && transformedBounds.Contains(Main.MouseScreen.ToPoint());

        if (!isMouseHovering)
            return;

        // TODO: add more info to the tooltip
        // - Probably crafting station + crafting conditions
        Main.instance.MouseText("");
        Main.HoverItem = drawnItem;
    }

    // TODO: aliasing still present - Fix by drawing lines in a pass before the icons without my custom UI matrix applied so the scaling isn't fucked
    private void DrawConnectionLine(SpriteBatch spriteBatch, Vector2 start, float offset, bool isVertical)
    {
        float connectionLineWidth = MathF.Max(2 / Scale, 2); // Ensure constant width if zoomed out
        var connectionLineColor = new Color(98, 105, 152);

        start.X -= connectionLineWidth / 2f;
        var end = start;

        if (isVertical)
        {
            end.X += connectionLineWidth;
            end.Y += offset;
        }
        else
        {
            end.X += offset;
            end.Y += connectionLineWidth;
        }

        // Helps with aliasing issues
        start = Utils.Round(start);
        end = Utils.Round(end);

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
