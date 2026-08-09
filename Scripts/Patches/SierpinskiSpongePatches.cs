using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using TiebaDIY.Scripts.Relics;

namespace TiebaDIY.Scripts.Patches;

[HarmonyPatch(
	typeof(RelicCmd),
	nameof(RelicCmd.Obtain),
	new Type[] { typeof(RelicModel), typeof(Player), typeof(int) })]
internal static class SierpinskiSpongeObtainPatch
{
	private static bool Prepare() => Entry.EnableSierpinskiSponge;

	private static void Postfix(Player player, ref Task<RelicModel> __result)
	{
		__result = ObtainWaxCopiesAfter(__result, player);
	}

	private static async Task<RelicModel> ObtainWaxCopiesAfter(Task<RelicModel> originalTask, Player player)
	{
		var obtainedRelic = await originalTask;
		if (!ShouldCopy(obtainedRelic))
			return obtainedRelic;

		var sponges = player.Relics
			.OfType<SierpinskiSponge>()
			.Where(static sponge => !sponge.IsMelted && sponge.Status != RelicStatus.Disabled)
			.ToList();

		foreach (var sponge in sponges)
		{
			if (!player.Relics.Contains(sponge))
				continue;

			var waxCopy = ModelDb.GetById<RelicModel>(obtainedRelic.Id).ToMutable();
			waxCopy.IsWax = true;
			sponge.Flash();
			await RelicCmd.Obtain(waxCopy, player);
		}

		return obtainedRelic;
	}

	private static bool ShouldCopy(RelicModel relic)
	{
		return //relic.Rarity != RelicRarity.Ancient &&
			relic is { IsWax: false, IsMelted: false };
	}
}