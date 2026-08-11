using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Cards;

[RegisterCard(typeof(NecrobinderCardPool))]
public sealed class IntoTheUnderworldTogether()
    : TiebaCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    private const string SelfDoomVar = "SelfDoom";
    private new const string PortraitPath = "res://TiebaDIY/images/cards/IntoTheUnderworldTogether.png";

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Ethereal,
    ];

    public override CardAssetProfile AssetProfile { get; } = new(
        PortraitPath: PortraitPath);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<DoomPower>(24m),
        new PowerVar<DoomPower>(SelfDoomVar, 9m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<DoomPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<DoomPower>(
            choiceContext,
            cardPlay.Target,
            DynamicVars.Doom.BaseValue,
            Owner.Creature,
            this);
        await PowerCmd.Apply<DoomPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars[SelfDoomVar].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Doom.UpgradeValueBy(7m);
    }
}
