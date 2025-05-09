using Terraria.DataStructures;

namespace SpiritReforged.Common.TileCommon.PresetTiles;

public abstract class WorkBenchTile : FurnitureTile
{
	public override void SetItemDefaults(ModItem item) => item.Item.value = Item.sellPrice(copper: 30);

	public override void AddItemRecipes(ModItem item)
	{
		if (CoreMaterial != ItemID.None)
			item.CreateRecipe()
			.AddIngredient(CoreMaterial, 10)
			.Register();
	}

	public override void StaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLavaDeath[Type] = true;
		Main.tileSolidTop[Type] = true;
		Main.tileTable[Type] = true;
		Main.tileNoAttach[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;
		TileID.Sets.IgnoredByNpcStepUp[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 2, 0);
		TileObjectData.newTile.Origin = new Point16(1, 0);
		TileObjectData.newTile.CoordinateHeights = [16];
		TileObjectData.addTile(Type);

		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
		AddMapEntry(new Color(100, 100, 60), Language.GetText("ItemName.WorkBench"));
		AdjTiles = [TileID.WorkBenches];
		DustType = -1;
	}
}
