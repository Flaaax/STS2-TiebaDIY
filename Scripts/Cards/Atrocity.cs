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

[RegisterCard(typeof(IroncladCardPool))]
public sealed class Atrocity()
    : TiebaCardModel(0, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    private new const string PortraitPath = "res://TiebaDIY/images/cards/Atrocity.png";

    public override CardAssetProfile AssetProfile { get; } = new(
        PortraitPath: PortraitPath);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<AtrocityPower>(2m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<AtrocityPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<AtrocityPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars[nameof(AtrocityPower)].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(AtrocityPower)].UpgradeValueBy(1m);
    }
}
