using ReLogic.Content.Sources;

namespace InterfacePlus.Utilities;

// TODO: move to terra util
public sealed class CustomContentSource(IContentSource contentSource, IReadOnlyDictionary<string, string> redirects) : IContentSource
{
    public IContentValidator ContentValidator { get => contentSource.ContentValidator; set => contentSource.ContentValidator = value; }
    public string FileWatcherPath { get => contentSource.FileWatcherPath; }
    public RejectedAssetCollection Rejections => contentSource.Rejections;

    public IEnumerable<string> EnumerateAssets()
    {
        return contentSource.EnumerateAssets().Select(RewritePath);
    }

    public string GetExtension(string assetName)
    {
        return contentSource.GetExtension(RewritePath(assetName));
    }

    public Stream OpenStream(string fullAssetName)
    {
        return contentSource.OpenStream(RewritePath(fullAssetName));
    }

    public void Refresh()
    {
        contentSource.Refresh();
    }

    private string RewritePath(string path)
    {
        foreach ((string from, string to) in redirects)
        {
            if (path.StartsWith(from, StringComparison.Ordinal))
                return path.Replace(from, to);
        }

        return path;
    }
}
