using Terraria.DataStructures;

namespace SpiritReforged.Content.Savanna.Items.Killifish;

[AutoloadEquip(EquipType.Head)]
public class KillieFishbowl : ModItem
{
	public override void SetDefaults()
	{
		Item.width = 32;
		Item.height = 28;
		Item.value = 500;
		Item.maxStack = Item.CommonMaxStack;
		Item.useTime = 10;
		Item.useAnimation = 15;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.createTile = ModContent.TileType<KillieFishbowlTile>();
		Item.useTurn = true;
		Item.autoReuse = true;
		Item.consumable = true;
		Item.vanity = true;
	}

	public override void AddRecipes() => CreateRecipe().AddIngredient(Mod.Find<ModItem>("KillifishItem").Type)
		.AddIngredient(ItemID.BottledWater).AddTile(TileID.WorkBenches).Register();
}

public class KillieFishbowlTile : ModTile
{
	bool activeAnimation = false;

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
		TileObjectData.newTile.UsesCustomCanPlace = true;
		TileObjectData.newTile.Height = 3;
		TileObjectData.newTile.Origin = new Point16(1, 2);
		TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
		Main.tileFrameImportant[Type] = Main.tileFrameImportant[TileID.FishBowl];
		Main.tileLavaDeath[Type] = Main.tileLavaDeath[TileID.FishBowl];
		Main.tileSolidTop[Type] = Main.tileSolidTop[TileID.FishBowl];
		Main.tileTable[Type] = Main.tileTable[TileID.FishBowl];
		TileObjectData.addTile(Type);

		DustType = DustID.Glass;
		AnimationFrameHeight = 54;

		LocalizedText name = CreateMapEntryName();
		AddMapEntry(new Color(200, 200, 200), name);
	}

	public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) => offsetY = 2;

	public override void AnimateTile(ref int frame, ref int frameCounter)
	{
		if (Main.rand.NextBool(100) && Main.netMode != NetmodeID.Server)
			activeAnimation = true;

		if (activeAnimation)
		{
			frameCounter++;
			if (frameCounter >= 6)
			{
				frameCounter = 0;
				frame++;
				frame %= 22;
			}

			if (frame == 16)
				activeAnimation = false;
		}
		else
		{
			frameCounter++;
			if (frameCounter >= 26)
			{
				frameCounter = 0;
				frame++;
				frame %= 22;
			}

			if (frame == 18)
				frame = 17;
		}
	}
}

internal class KillieFishbowlPlayer : ModPlayer
{
	public short counter;

	public override void FrameEffects()
	{
		if (counter != 5 * KillieFishbowlLayer.FrameDuration || Main.rand.NextBool(30))
			counter = (short)(++counter % (KillieFishbowlLayer.NumFrames * KillieFishbowlLayer.FrameDuration));

		if (counter == 7 * KillieFishbowlLayer.FrameDuration && !Main.rand.NextBool(8))
			counter = 0;
	}
}

internal class KillieFishbowlLayer : PlayerDrawLayer
{
	public const int NumFrames = 18;
	public const int FrameDuration = 6;

	private static Asset<Texture2D> Texture;

	public override void Load() => Texture = ModContent.Request<Texture2D>(ModContent.GetInstance<KillieFishbowl>().Texture + "_Head2");
	public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.FaceAcc);

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		var player = drawInfo.drawPlayer;
		if (player.dead || player.invis && !player.isDisplayDollOrInanimate)
			return;

		if (Equipped(player))
		{
			var helmetOffset = drawInfo.helmetOffset;
			var bobbing = Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height] * player.gravDir;
			float yOff = (player.gravDir == -1) ? 8f : -12f;

			var position = helmetOffset + new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - player.bodyFrame.Width / 2 + player.width / 2),
				(int)(drawInfo.Position.Y - Main.screenPosition.Y + player.height - player.bodyFrame.Height + yOff)) + player.headPosition + drawInfo.headVect + bobbing;

			int frame = (int)(player.GetModPlayer<KillieFishbowlPlayer>().counter / (float)FrameDuration) % NumFrames;
			var source = Texture.Value.Frame(1, NumFrames, 0, frame, 0, -2);

			var data = new DrawData(Texture.Value, position, source, drawInfo.colorArmorHead, player.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect);
			data.shader = drawInfo.cHead;

			drawInfo.DrawDataCache.Add(data);
		}
	}

	private static bool Equipped(Player player)
	{
		var vHead = player.armor[10];
		if (vHead != null && !vHead.IsAir)
			return vHead.type == ModContent.ItemType<KillieFishbowl>();

		var head = player.armor[0];
		return head != null && head.type == ModContent.ItemType<KillieFishbowl>();
	}
}