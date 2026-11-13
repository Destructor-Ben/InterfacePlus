using Terraria.Map;

namespace InterfacePlus.Content.CraftTree.CraftingRequirements;

// We ignore crafting stations acting as each other because it's complex
public record CraftingStationRequirement(int TileType) : ICraftingRequirement
{
    public Item DrawnItem => new(DrawnItemType);
    public string Description => ModInstance.GetLocalization("CraftingRequirement.CraftingStation").Format(TileName);
    public string TileName => Lang.GetMapObjectName(MapHelper.TileToLookup(TileType, TileStyle));

    public int TileStyle
    {
        get {
            if (TileType == TileID.DemonAltar)
                return WorldGen.crimson ? 1 : 0;

            if (TileType == TileID.Anvils)
                return WorldGen.SavedOreTiers.Iron == TileID.Iron ? 0 : 1;

            if (TileType == TileID.MythrilAnvil)
                return WorldGen.SavedOreTiers.Mythril == TileID.Mythril ? 0 : 1;

            if (TileType == TileID.AdamantiteForge)
                return WorldGen.SavedOreTiers.Adamantite == TileID.Adamantite ? 0 : 1;

            return 0;
        }
    }

    // Unfortunately, hardcoded
    public int DrawnItemType => TileType switch
    {
        TileID.WorkBenches          => ItemID.WorkBench,
        TileID.Furnaces             => ItemID.Furnace,
        TileID.Anvils               => WorldGen.SavedOreTiers.Iron == TileID.Iron ? ItemID.IronAnvil : ItemID.LeadAnvil,
        TileID.Bottles              => ItemID.Bottle,
        TileID.Sawmill              => ItemID.Sawmill,
        TileID.Loom                 => ItemID.Loom,
        TileID.CookingPots          => ItemID.CookingPot,
        TileID.TinkerersWorkbench   => ItemID.TinkerersWorkshop,
        TileID.ImbuingStation       => ItemID.ImbuingStation,
        TileID.DyeVat               => ItemID.DyeVat,
        TileID.HeavyWorkBench       => ItemID.HeavyWorkBench,
        TileID.Hellforge            => ItemID.Hellforge,
        TileID.DemonAltar           => WorldGen.crimson ? ItemID.CrimsonAltar : ItemID.DemonAltar,
        TileID.MythrilAnvil         => WorldGen.SavedOreTiers.Mythril == TileID.Mythril ? ItemID.MythrilAnvil : ItemID.OrichalcumAnvil,
        TileID.AdamantiteForge      => WorldGen.SavedOreTiers.Adamantite == TileID.Adamantite ? ItemID.AdamantiteForge : ItemID.TitaniumForge,
        TileID.Bookcases            => ItemID.Bookcase,
        TileID.CrystalBall          => ItemID.CrystalBall,
        TileID.Autohammer           => ItemID.Autohammer,
        TileID.LunarCraftingStation => ItemID.LunarCraftingStation,
        // TODO: add more crafting stations
        _                           => ItemID.None,
    };

    public void DrawExtra(SpriteBatch spriteBatch, Vector2 slotTopLeft, Vector2 slotSize)
    {
        if (DrawnItemType != ItemID.None)
            return;

        UnknownRequirement.DrawUnknownIcon(spriteBatch, slotTopLeft, slotSize);
    }
}
