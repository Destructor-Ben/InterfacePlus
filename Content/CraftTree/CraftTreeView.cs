using Terraria.UI;

namespace InterfacePlus.Content.CraftTree;

// TODO: copy the camera control UI from the unfinished incremental mod
// TODO: give this a border + proper background
public class CraftTreeView : UIElement
{
    public Vector2 Position = Vector2.Zero;
    public float Scale = 1f;

    public static float TranslateSpeed => 3f;
    public static float ScaleSpeed => 1.02f;

    private static int IconVerticalShift => 75;
    private static int IconHorizontalShift => 75;
    private static bool EnableCentering => true;

    public CraftTreeView()
    {
        OverflowHidden = true;
    }

    public void Reset()
    {
        RemoveAllChildren();
    }

    public void SetItem(int itemType)
    {
        var innerDimension = GetInnerDimensions();
        var position = new Vector2(innerDimension.Width / 2, IconVerticalShift);

        var craftTree = CraftTreeTraverser.CreateCraftTree(itemType);
        AddCraftTreeIcon(craftTree, position);
    }

    private void AddCraftTreeIcon(CraftTreeNode node, Vector2 position, CraftTreeIcon parent = null)
    {
        var icon = new CraftTreeIcon
        {
            ItemType = node.ItemType,
            ParentIcon = parent,
            Left = { Pixels = position.X },
            Top = { Pixels = position.Y },
        };
        Append(icon);

        var childPosition = position + Vector2.UnitY * IconVerticalShift;

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

    private void DrawCraftTreeIcons(SpriteBatch spriteBatch)
    {
        // TODO: make craft tree icons just drawn here instead of children
    }

    private Matrix GetMatrix()
    {
        var transform = Matrix.Identity;

        // Shift centre of screen to (0, 0) so scaling isn't weird
        transform *= Matrix.CreateTranslation(-Main.screenWidth / 2f, -Main.screenHeight / 2f, 0f);

        // Camera movement
        transform *= Matrix.CreateTranslation(-Position.X, -Position.Y, 0f);

        // Camera scale
        transform *= Matrix.CreateScale(Scale) * Main.UIScaleMatrix;

        // Shift back
        transform *= Matrix.CreateTranslation(Main.screenWidth / 2f, Main.screenHeight / 2f, 0f);

        return transform;
    }

    // Modified from UIElement.Draw()
    protected override void DrawChildren(SpriteBatch spriteBatch)
    {
        var rasterizerState = spriteBatch.GraphicsDevice.RasterizerState;
        var scissorRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;

        spriteBatch.End();
        spriteBatch.GraphicsDevice.ScissorRectangle = GetClippingRectangle(spriteBatch);
        spriteBatch.GraphicsDevice.RasterizerState = OverflowHiddenRasterizerState;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, OverflowHiddenRasterizerState, null, GetMatrix());

        base.DrawChildren(spriteBatch);
        DrawCraftTreeIcons(spriteBatch);

        spriteBatch.End();
        spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;
        spriteBatch.GraphicsDevice.RasterizerState = rasterizerState;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, rasterizerState, null, Main.UIScaleMatrix);
    }
}
