using InterfacePlus.Utilities;
using log4net;
using ReLogic.Content.Sources;

namespace InterfacePlus;

public static class GlobalUsings
{
    public static InterfacePlusMod ModInstance => ModContent.GetInstance<InterfacePlusMod>();
    public static ILog ModLogger => ModInstance.Logger;
}

public class InterfacePlusMod : Mod
{
    public override IContentSource CreateDefaultContentSource()
    {
        return new CustomContentSource(
            base.CreateDefaultContentSource(),
            new Dictionary<string, string>
            {
                ["Content"] = "Assets/Textures",
            }
        );
    }
}
