﻿using System.Linq;

namespace SpiritReforged.Common.PlayerCommon;

/// <summary> ModPlayer class managing extra additive or alphablend draw calls on top of a player, due to the inflexibility of playerlayers. </summary>
public class ExtraDrawOnPlayer : ModPlayer
{
	public enum DrawType
	{
		AlphaBlend,
		Additive,
		NonPremultiplied
	}

	public delegate void DrawAction(SpriteBatch spriteBatch);

	public IDictionary<DrawAction, DrawType> DrawDict = new Dictionary<DrawAction, DrawType>();

	public override void Load() => On_Main.DrawPlayers_AfterProjectiles += (On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self) =>
	{
		orig(self);
		DrawPlayers();
	};

	public override void ResetEffects() => DrawDict = new Dictionary<DrawAction, DrawType>();

	/// <summary> Check if any of the draw calls on the player have the specified draw type. </summary>
	public bool AnyOfType(DrawType Type)
	{
		foreach (KeyValuePair<DrawAction, DrawType> kvp in DrawDict)
			if (kvp.Value == Type)
				return true;

		return false;
	}

	/// <summary> Draw all draw calls on the player with a specified draw type. </summary>
	public void DrawAllCallsOfType(SpriteBatch spriteBatch, DrawType Type)
	{
		foreach (KeyValuePair<DrawAction, DrawType> kvp in DrawDict)
			if (kvp.Value == Type)
				kvp.Key.Invoke(spriteBatch);
	}

	/// <summary>
	/// Static method called in a detour after drawing all players.<br />
	/// Iterates through all players, adding the extra calls to a list, then draws them in seperate batches, if any players have a call of the type.
	/// </summary>
	public static void DrawPlayers()
	{
		var additiveCallPlayers = new List<ExtraDrawOnPlayer>();
		var alphaBlendCallPlayers = new List<ExtraDrawOnPlayer>();
		var nonPremultipliedCallPlayers = new List<ExtraDrawOnPlayer>();

		foreach (Player player in Main.player.Where(x => x.active && x != null))
		{
			if (player.GetModPlayer<ExtraDrawOnPlayer>().AnyOfType(DrawType.Additive))
				additiveCallPlayers.Add(player.GetModPlayer<ExtraDrawOnPlayer>());

			if (player.GetModPlayer<ExtraDrawOnPlayer>().AnyOfType(DrawType.AlphaBlend))
				alphaBlendCallPlayers.Add(player.GetModPlayer<ExtraDrawOnPlayer>());

			if (player.GetModPlayer<ExtraDrawOnPlayer>().AnyOfType(DrawType.NonPremultiplied))
				nonPremultipliedCallPlayers.Add(player.GetModPlayer<ExtraDrawOnPlayer>());
		}

		if (nonPremultipliedCallPlayers.Count != 0)
		{

			Main.spriteBatch.Begin(SpriteSortMode.Deferred, AssetLoader.NonPremultipliedAlphaFix, SamplerState.PointClamp, null, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
			foreach (ExtraDrawOnPlayer player in nonPremultipliedCallPlayers)
				player.DrawAllCallsOfType(Main.spriteBatch, DrawType.NonPremultiplied);

			Main.spriteBatch.End();
		}

		if (alphaBlendCallPlayers.Count != 0)
		{
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
			foreach (ExtraDrawOnPlayer player in alphaBlendCallPlayers)
				player.DrawAllCallsOfType(Main.spriteBatch, DrawType.AlphaBlend);

			Main.spriteBatch.End();
		}

		if (additiveCallPlayers.Count != 0)
		{
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, null, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
			foreach (ExtraDrawOnPlayer player in additiveCallPlayers)
				player.DrawAllCallsOfType(Main.spriteBatch, DrawType.Additive);

			Main.spriteBatch.End();
		}
	}
}