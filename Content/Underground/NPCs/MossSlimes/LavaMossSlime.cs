﻿namespace SpiritReforged.Content.Underground.NPCs.MossSlimes;

internal class LavaMossSlime : MossSlime
{
	protected override Vector3 LightColor => new(0.5f, 0.15f, 0);
	protected override int MossType => ItemID.LavaMoss;
	protected override HashSet<int> TileTypes => [TileID.LavaMossBlock, TileID.LavaMossBrick];
	protected override int DustType => DustID.Torch;
}
