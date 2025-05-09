using MonoMod.Cil;
using SpiritReforged.Common.ItemCommon.FloatingItem;
using SpiritReforged.Common.Particle;
using SpiritReforged.Common.TileCommon;
using SpiritReforged.Common.TileCommon.DrawPreviewHook;
using SpiritReforged.Content.Particles;
using Terraria.Audio;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Ocean.Items.KoiTotem;

public class KoiTotem : FloatingItem
{
	private static Asset<Texture2D> CursorTexture;
	public static float CursorOpacity { get; private set; }

	public override float SpawnWeight => 0.005f;
	public override float Weight => base.Weight * 0.9f;
	public override float Bouyancy => base.Bouyancy * 1.07f;

	public override void Load()
	{
		if (!Main.dedServ)
			CursorTexture = ModContent.Request<Texture2D>(Texture.Remove(Texture.Length - Name.Length) + "Cursor_Plus");

		IL_Player.GetItem_FillIntoOccupiedSlot += RemovePickupSound;
		On_Player.ItemCheck_CheckFishingBobber_PickAndConsumeBait += CheckBait;
		On_Main.DrawInterface_40_InteractItemIcon += DrawPlusIcon;
	}

	private static void RemovePickupSound(ILContext il)
	{
		var c = new ILCursor(il);
		if (!c.TryGotoNext(MoveType.After, x => x.MatchLdcI4(7))) //Match the sound id
			return;

		//Remove the item pickup sound when replenishing bait to an existing stack
		c.EmitDelegate((int sound) => (CursorOpacity == 1) ? -1 : sound);
	}

	private static void DrawPlusIcon(On_Main.orig_DrawInterface_40_InteractItemIcon orig, Main self)
	{
		orig(self);

		if (CursorOpacity > 0 && Main.LocalPlayer.cursorItemIconID > 0)
		{
			var pos = Main.MouseScreen + new Vector2(6) + TextureAssets.Item[Main.LocalPlayer.cursorItemIconID].Size();
			Main.spriteBatch.Draw(CursorTexture.Value, pos, null, Color.White * CursorOpacity * 2, 0, Vector2.Zero, Main.cursorScale, SpriteEffects.None, 0f);
			CursorOpacity = MathHelper.Max(CursorOpacity - .025f, 0);
		}
	}

	private static void CheckBait(On_Player.orig_ItemCheck_CheckFishingBobber_PickAndConsumeBait orig, Player self, Projectile bobber, out bool pullTheBobber, out int baitTypeUsed)
	{
		orig(self, bobber, out pullTheBobber, out baitTypeUsed);

		if (pullTheBobber && self.HasBuff<KoiTotemBuff>() && Main.rand.NextBool(10))
		{
			CursorOpacity = 1;
			self.GetItem(self.whoAmI, new Item(baitTypeUsed), new GetItemSettings(false, true));
		}
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<KoiTotemTile>());
		Item.value = Item.sellPrice(gold: 1);
		Item.rare = ItemRarityID.Blue;
	}
}

public class KoiTotemTile : ModTile, IDrawPreview
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.Height = 4;
		TileObjectData.newTile.Origin = new(0, 3);
		TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 18];
		TileObjectData.newTile.CoordinateWidth = 18;
		TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
		TileObjectData.newTile.StyleWrapLimit = 2; 
		TileObjectData.newTile.StyleMultiplier = 2; 
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft; 
		TileObjectData.addAlternate(1); 
		TileObjectData.addTile(Type);

		DustType = DustID.Ash;
		AddMapEntry(new Color(107, 90, 64), CreateMapEntryName());
	}

	public override void NearbyEffects(int i, int j, bool closer)
	{
		var player = Main.LocalPlayer;

		if (!closer)
		{
			if (!player.dead)
				player.AddBuff(ModContent.BuffType<KoiTotemBuff>(), 12);
		}
		else if (TileObjectData.IsTopLeft(i, j) && KoiTotem.CursorOpacity > 0) //Create fancy visuals when bait is replenished
		{
			var pos = new Vector2(i, j).ToWorldCoordinates(0, 64);

			if (Main.rand.NextBool())
			{
				var color = Color.Lerp(Color.LightBlue, Color.Cyan, Main.rand.NextFloat());
				float magnitude = Main.rand.NextFloat();

				ParticleHandler.SpawnParticle(new GlowParticle(pos + new Vector2(Main.rand.NextFloat(32), 0), Vector2.UnitY * -magnitude,
					color, (1f - magnitude) * .25f, Main.rand.Next(30, 120), 5, extraUpdateAction: delegate (Particle p)
					{ p.Velocity = p.Velocity.RotatedBy(Main.rand.NextFloat(-.1f, .1f)); }));
			}

			if (KoiTotem.CursorOpacity > .9f)
			{
				const string path = "SpiritReforged/Assets/SFX/Ambient/MagicFeedback";

				ParticleHandler.SpawnParticle(new DissipatingImage(pos + new Vector2(18, 0), Color.Cyan * .15f, 0, .25f, 1f, "Bloom", 120));

				SoundEngine.PlaySound(new SoundStyle(path + 1) with { Volume = .3f, PitchRange = (-1, -.75f) }, pos);
				SoundEngine.PlaySound(new SoundStyle(path + 2) with { Volume = .4f, PitchRange = (-.65f, -.35f) }, pos);
			}
		}
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		if (!TileExtensions.GetVisualInfo(i, j, out var color, out var texture))
			return false;

		var t = Main.tile[i, j];
		var frame = new Point(t.TileFrameX, t.TileFrameY);
		var source = new Rectangle(frame.X, frame.Y, 18, (frame.Y == 54) ? 18 : 16);
		int offX = (frame.X % TileObjectData.GetTileData(Type, 0).CoordinateFullWidth == 0) ? -2 : 0;
		var position = new Vector2(i, j) * 16 - Main.screenPosition + TileExtensions.TileOffset + new Vector2(offX, 0);

		spriteBatch.Draw(texture, position, source, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

		return false;
	}

	public void DrawPreview(SpriteBatch spriteBatch, TileObjectPreviewData op, Vector2 position)
	{
		var texture = TextureAssets.Tile[op.Type].Value;
		var data = TileObjectData.GetTileData(op.Type, op.Style, op.Alternate);
		var color = ((op[0, 0] == 1) ? Color.White : Color.Red * .7f) * .5f;

		int style = data.CalculatePlacementStyle(op.Style, op.Alternate, op.Random);

		for (int frameX = 0; frameX < 2; frameX++)
		{
			for (int frameY = 0; frameY < 4; frameY++)
			{
				(int x, int y) = (op.Coordinates.X + frameX, op.Coordinates.Y + frameY);

				var source = new Rectangle(frameX * 20 + style * data.CoordinateFullWidth, frameY * 18, 18, (frameY == 3) ? 18 : 16);
				int offX = (frameX == 0) ? -2 : 0;
				var drawPos = new Vector2(x, y) * 16 - Main.screenPosition + TileExtensions.TileOffset + new Vector2(offX, 0);

				spriteBatch.Draw(texture, drawPos, source, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
			}
		}
	}
}
