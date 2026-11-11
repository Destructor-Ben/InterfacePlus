namespace InterfacePlus.Utilities;

public static class MathsUtils
{
    public static Rectangle Transform(this Rectangle rect, Matrix transform)
    {
        var cornerA = rect.TopLeft();
        var cornerB = rect.BottomRight();

        var transformedCornerA = Vector2.Transform(cornerA, transform);
        var transformedCornerB = Vector2.Transform(cornerB, transform);

        return Utils.CornerRectangle(transformedCornerA, transformedCornerB);
    }
}
