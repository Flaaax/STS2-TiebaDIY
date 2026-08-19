using FBECore.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Cards;

[RegisterCard(typeof(CurseCardPool))]
public sealed class Muramasa()
	: TiebaCardModel(2, CardType.Curse, CardRarity.Curse, TargetType.AnyEnemy)
{
	private new const string PortraitPath = "res://TiebaDIY/images/cards/Muramasa.png";

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords =>
	[
		FBECoreKeywords.Automatic,
	];

	public override CardAssetProfile AssetProfile { get; } = new(
		PortraitPath: PortraitPath);

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(25m, ValueProp.Move),
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.Static(StaticHoverTip.Fatal),
	];

	public override async Task AfterCardDrawn(
		PlayerChoiceContext choiceContext,
		CardModel card,
		bool fromHandDraw)
	{
		if (!ReferenceEquals(card, this))
			return;

		var target = CombatState?.HittableEnemies
			.OrderBy(static enemy => enemy.CurrentHp)
			.FirstOrDefault();

		await CardPileCmd.Add(this, PileType.Play);
		await CardCmd.AutoPlay(choiceContext, this, target);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target);

		var shouldTriggerFatal = cardPlay.Target.Powers
			.All(static power => power.ShouldOwnerDeathTriggerFatal());

#if STS2_Stable
        var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this);
#elif STS2_Beta
		var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay);
#endif

		var attackCommand = await attack
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);

		if (shouldTriggerFatal && attackCommand.Results
			    .SelectMany(static results => results)
			    .Any(static result => result.WasTargetKilled))
		{
			await Cmd.Wait(0.25f);

			TalkCmd.Play(
				new LocString("cards", $"{Id.Entry}.fatal"),
				Owner.Creature,
				VfxColor.Purple);

			var curse = Owner.RunState.Rng.Niche.NextItem(
				ModelDb.CardPool<CurseCardPool>()
					.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
					.Where(static card => card.CanBeGeneratedByModifiers));

			if (curse is null)
			{
				return;
			}

			await CardPileCmd.AddCursesToDeck([curse], Owner);

			// await Cmd.Wait(0.5f);
		}
	}
}
