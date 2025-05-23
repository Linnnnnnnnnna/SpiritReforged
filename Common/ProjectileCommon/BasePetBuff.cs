﻿using SpiritReforged.Common.PlayerCommon;

namespace SpiritReforged.Common.ProjectileCommon;

public abstract class BasePetBuff<T> : ModBuff where T : ModProjectile
{
	protected virtual bool IsLightPet => false;

	public sealed override void SetStaticDefaults()
	{
		Main.buffNoTimeDisplay[Type] = true;

		if (IsLightPet)
			Main.lightPet[Type] = true;
		else
			Main.vanityPet[Type] = true;
	}

	public sealed override void Update(Player player, ref int buffIndex)
	{
		player.buffTime[buffIndex] = 18000;
		SetPetFlag(player, player.GetModPlayer<PetPlayer>());

		bool petProjectileNotSpawned = player.ownedProjectileCounts[ModContent.ProjectileType<T>()] <= 0;
		if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
			Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, Vector2.Zero, ModContent.ProjectileType<T>(), 0, 0f, player.whoAmI);
	}

	public virtual void SetPetFlag(Player player, PetPlayer petPlayer)
	{
		if (!petPlayer.pets.ContainsKey(ModContent.ProjectileType<T>()))
			petPlayer.pets.Add(ModContent.ProjectileType<T>(), true);

		petPlayer.pets[ModContent.ProjectileType<T>()] = true;
	}
}
