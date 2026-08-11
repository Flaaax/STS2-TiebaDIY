using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using TiebaDIY.Scripts.Powers;

namespace TiebaDIY.Scripts.Cards;

[RegisterCard(typeof(DefectCardPool))]
public sealed class HotStart()
	: TiebaCardModel(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
	private new const string PortraitPath = "res://TiebaDIY/images/cards/HotStart.png";

	public override CardAssetProfile AssetProfile { get; } = new(
		PortraitPath: PortraitPath);

	public override IEnumerable<CardKeyword> CanonicalKeywords =>
	[
		//CardKeyword.Innate,
	];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new PowerVar<HotStartPower>(2m),
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.Static(StaticHoverTip.Channeling),
		// HoverTipFactory.FromPower<HotStartPower>(),
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
		await PowerCmd.Apply<HotStartPower>(
			choiceContext,
			Owner.Creature,
			DynamicVars[nameof(HotStartPower)].BaseValue,
			Owner.Creature,
			this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars[nameof(HotStartPower)].UpgradeValueBy(1m);
	}
}