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

[RegisterCard(typeof(NecrobinderCardPool))]
public sealed class TimesTwo()
	: TiebaCardModel(2, CardType.Power, CardRarity.Rare, TargetType.Self), ITiebaModel
{
	public bool IsImba => true;
	private new const string PortraitPath = "res://TiebaDIY/images/cards/TimesTwo.png";

	public override CardAssetProfile AssetProfile { get; } = new(
		PortraitPath: PortraitPath);

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new PowerVar<TimesTwoPower>(1m),
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		//HoverTipFactory.FromPower<TimesTwoPower>(),
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
		await PowerCmd.Apply<TimesTwoPower>(
			choiceContext,
			Owner.Creature,
			DynamicVars[nameof(TimesTwoPower)].BaseValue,
			Owner.Creature,
			this);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}