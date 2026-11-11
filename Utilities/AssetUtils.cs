namespace InterfacePlus.Utilities;

public static class AssetUtils
{
    public static Asset<T> WaitForMe<T>(this Asset<T> asset) where T : class
    {
        asset.Wait();
        return asset;
    }
}
