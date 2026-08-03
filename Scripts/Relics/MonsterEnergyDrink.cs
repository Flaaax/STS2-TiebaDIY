using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool), StableEntryStem = "MonsterEnergyDrink")]
public sealed class MonsterEnergyDrink : TiebaRelicModel
{
	private const int PoorSleepCount = 2;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new EnergyVar(1),
		new CardsVar(PoorSleepCount),
	];

	protected override bool IncludeEnergyHoverTip => true;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		HoverTipFactory.FromCardWithCardHoverTips<PoorSleep>();

	public override async Task AfterEnergyReset(Player player)
	{
		if (player != Owner)
			return;

		Flash();
		await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
	}

	public override async Task AfterRestSiteHeal(Player player, bool isMimicked)
	{
		if (player != Owner || isMimicked)
			return;

		Flash();
		await CardPileCmd.AddCursesToDeck(
			Enumerable.Repeat(ModelDb.Card<PoorSleep>(), DynamicVars.Cards.IntValue),
			Owner);
	}

	public override IReadOnlyList<LocString> ModifyExtraRestSiteHealText(
		Player player,
		IReadOnlyList<LocString> currentExtraText)
	{
		if (player != Owner || !LocalContext.IsMe(Owner))
			return currentExtraText;

		if (AdditionalRestSiteHealText is null)
		{
			Log.Warn("somehow AdditionalRestSiteHealText is null");
			return currentExtraText;
		}

		return [.. currentExtraText, AdditionalRestSiteHealText];
	}
}