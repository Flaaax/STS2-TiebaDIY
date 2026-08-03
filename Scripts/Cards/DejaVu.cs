using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Cards;

[RegisterCard(typeof(CurseCardPool))]
public sealed class DejaVu()
    : TiebaCardModel(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    private const string ChanceVar = "Chance";
    private new const string PortraitPath = "res://TiebaDIY/images/cards/DejaVu.png";

    private CardModel? _invalidatedCard;

    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable,
    ];

    public override CardAssetProfile AssetProfile { get; } = new(
        PortraitPath: PortraitPath);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(ChanceVar, 10m),
    ];

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (!ReferenceEquals(_invalidatedCard, card))
            return playCount;

        AssertMutable();
        _invalidatedCard = null;
        return 0;
    }

#if STS2_0_110_0
    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        CardLocation location)
    {
        if (!TryInvalidate(card))
            return location;

        location.player = card.Owner;
        location.pileType = PileType.Discard;
        location.position = CardPilePosition.Bottom;
        return location;
    }
#elif STS2_0_107_1
    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        PileType pileType,
        CardPilePosition position)
    {
        return TryInvalidate(card)
            ? (PileType.Discard, CardPilePosition.Bottom)
            : (pileType, position);
    }
#endif

    private bool TryInvalidate(CardModel card)
    {
        if (Pile?.Type != PileType.Hand ||
            !ReferenceEquals(card.Owner, Owner) ||
            ReferenceEquals(card, this))
        {
            return false;
        }

        if (Owner.RunState.Rng.CombatCardSelection.NextInt(100) >= DynamicVars[ChanceVar].IntValue)
            return false;

        AssertMutable();
        _invalidatedCard = card;
        return true;
    }
}
