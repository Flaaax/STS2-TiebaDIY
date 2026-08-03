using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Cards;

[RegisterCard(typeof(IroncladCardPool))]
public sealed class IronWavePro()
    : TiebaCardModel(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    private new const string PortraitPath = "res://TiebaDIY/images/cards/IronWavePro.png";

    public override bool GainsBlock => true;

    protected override bool HasEnergyCostX => true;

    public override CardAssetProfile AssetProfile { get; } = new(
        PortraitPath: PortraitPath);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new BlockVar(5m, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var repeatCount = ResolveEnergyXValue();
        for (var i = 0; i < repeatCount; i++)
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

#if STS2_0_107_1
        var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(repeatCount)
            .FromCard(this);
#elif STS2_0_110_0
        var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(repeatCount)
            .FromCard(this, cardPlay);
#endif

        await attack.Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_flying_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
