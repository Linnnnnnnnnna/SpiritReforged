﻿using SpiritReforged.Common.WorldGeneration;
using System.Linq;
using Terraria.GameContent.Biomes.CaveHouse;

namespace SpiritReforged.Content.Snow.Frostbite;

internal class IceLibrary : ILoadable
{
	/// <summary> The maximum number of <b>guaranteed</b> Frostbite tomes in this world. </summary>
	public static int GenCountMax => WorldGen.GetWorldSize() == WorldGen.WorldSize.Large ? 2 : 1;

	/// <summary> The number of Frostbite tomes generated. </summary>
	[WorldBound]
	private static int GenCount;

	public void Load(Mod mod) => On_HouseBuilder.FillRooms += AddBooks;
	private static void AddBooks(On_HouseBuilder.orig_FillRooms orig, HouseBuilder self)
	{
		if (self.Type != HouseType.Ice)
		{
			orig(self);
			return;
		}

		bool canGen = GenCount < GenCountMax || WorldGen.genRand.NextBool(5);

		foreach (var room in self.Rooms)
		{
			if (canGen || WorldGen.genRand.NextBool(4 + WorldGen.GetWorldSize()))
			{
				int length = WorldGen.genRand.Next(3, 6);
				int x = room.X + 1 + WorldGen.genRand.Next(room.Width - (length + 1));
				int y = room.Y + 2;

				PlaceShelf(x, y, new Point(length, WorldGen.genRand.Next(1, 3)), self.PlatformStyle, ref canGen);
			}
		}

		orig(self);
	}

	static void PlaceShelf(int originX, int originY, Point size, int style, ref bool canGen)
	{
		HashSet<Point> safe = []; //Tracks empty shelves for Frostbite gen

		for (int x = originX; x < originX + size.X; x++)
		{
			for (int j = 0; j < size.Y; j++)
			{
				int y = originY + j * 2;
				int bookStyle = WorldGen.genRand.Next(FrozenBooks.Styles - 1);

				WorldGen.PlaceTile(x, y, (bookStyle > 5) ? ModContent.TileType<FrostedPlatform>() : TileID.Platforms, style: (bookStyle > 5) ? 0 : style);

				if (WorldGen.genRand.NextFloat() < 0.66f)
					WorldGen.PlaceTile(x, y - 1, ModContent.TileType<FrozenBooks>(), style: bookStyle);
				else
					safe.Add(new Point(x, y - 1));
			}
		}

		if (!canGen)
			return;

		foreach (var pt in safe.OrderBy(x => WorldGen.genRand.Next(safe.Count))) //Place a tome in a random empty location
		{
			WorldGen.PlaceTile(pt.X, pt.Y + 1, ModContent.TileType<FrostedPlatform>());

			int type = ModContent.TileType<FrozenBooks>();
			WorldGen.PlaceTile(pt.X, pt.Y, ModContent.TileType<FrozenBooks>(), style: FrozenBooks.Styles - 1);

			if (Framing.GetTileSafely(pt).TileType == type)
			{
				canGen = false;
				GenCount++;

				break;
			}
		}
	}

	public void Unload() { }
}