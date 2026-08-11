using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Cards;

[RegisterCard(typeof(CurseCardPool))]
public sealed class Trance()
    : TiebaCardModel(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    private new const string PortraitPath = "res://TiebaDIY/images/cards/Trance.png";

    private static readonly PileType[] AffectedPiles =
    [
        PileType.Hand,
        PileType.Draw,
        PileType.Discard,
        PileType.Exhaust,
    ];

    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable,
    ];

    public override CardAssetProfile AssetProfile { get; } = new(
        PortraitPath: PortraitPath);

    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        if (!ReferenceEquals(card, this))
            return;

        var transformations = AffectedPiles
            .SelectMany(pileType => pileType.GetPile(Owner).Cards)
            .Where(static card =>
                card is not Trance &&
                (card.Type is CardType.Curse or CardType.Status))
            .Select(static card => new CardTransformation(card))
            .ToList();

        await CardCmd.Transform(
            transformations,
            Owner.RunState.Rng.CombatCardSelection,
            CardPreviewStyle.None);
    }
}
