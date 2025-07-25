﻿using SpiritReforged.Common.TileCommon.PresetTiles;

namespace SpiritReforged.Content.Savanna.Tiles;

public class SavannaVine : VineTile
{
	public override void PreAddObjectData()
	{
		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<SavannaGrass>()];

		AddMapEntry(new Color(24, 135, 28));
		DustType = DustID.JunglePlants;
	}
}

public class SavannaVineCorrupt : SavannaVine
{
	public override void PreAddObjectData()
	{
		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<SavannaGrassCorrupt>()];

		TileID.Sets.AddCorruptionTile(Type);
		TileID.Sets.Corrupt[Type] = true;

		AddMapEntry(new(109, 106, 174));
		DustType = DustID.Corruption;
	}
}

public class SavannaVineCrimson : SavannaVine
{
	public override void PreAddObjectData()
	{
		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<SavannaGrassCrimson>()];

		TileID.Sets.AddCrimsonTile(Type);
		TileID.Sets.Crimson[Type] = true;

		AddMapEntry(new(183, 69, 68));
		DustType = DustID.CrimsonPlants;
	}
}

public class SavannaVineHallow : SavannaVine
{
	public override void PreAddObjectData()
	{
		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<SavannaGrassHallow>()];

		TileID.Sets.Hallow[Type] = true;
		TileID.Sets.HallowBiome[Type] = 1;

		AddMapEntry(new(78, 193, 227));
		DustType = DustID.HallowedPlants;
	}
}