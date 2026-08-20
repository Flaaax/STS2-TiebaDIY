using FBECore.Scripts.Keywords;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Cards;

[RegisterCard(typeof(CurseCardPool))]
public sealed class OralFibrosis()
    : TiebaCardModel(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    private const string EnergyThresholdVar = "EnergyThreshold";
    private new const string PortraitPath = "res://TiebaDIY/images/cards/OralFibrosis.png";

    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedByModifiers => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable,
        FBECoreKeywords.WhileInHand,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(EnergyThresholdVar, 3),
    ];

    public override CardAssetProfile AssetProfile { get; } = new(
        PortraitPath: PortraitPath);

    public override bool ShouldPlay(CardModel card, AutoPlayType _)
    {
        if (card.Owner != Owner || Pile?.Type != PileType.Hand)
            return true;

        return card.EnergyCost.GetAmountToSpend() <
               DynamicVars[EnergyThresholdVar].IntValue;
    }
}
