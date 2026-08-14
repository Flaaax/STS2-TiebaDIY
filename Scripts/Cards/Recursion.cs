using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Cards;

[RegisterCard(typeof(DefectCardPool))]
public sealed class Recursion()
	: TiebaCardModel(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
	private new const string PortraitPath = "res://TiebaDIY/images/cards/Recursion.png";

	public override CardAssetProfile AssetProfile { get; } = new(
		PortraitPath: PortraitPath);

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.Static(StaticHoverTip.Evoke),
		HoverTipFactory.Static(StaticHoverTip.Channeling),
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner.PlayerCombatState!.OrbQueue.Orbs.Count == 0)
		{
			return;
		}

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		var copiedOrb = (OrbModel)Owner.PlayerCombatState.OrbQueue.Orbs.Last()
			.ClonePreservingMutability();
		await OrbCmd.EvokeNext(choiceContext, Owner);
		await OrbCmd.Channel(choiceContext, copiedOrb, Owner);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}
