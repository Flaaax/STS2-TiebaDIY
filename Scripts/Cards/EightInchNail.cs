using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Cards;

[RegisterCard(typeof(CurseCardPool))]
public sealed class EightInchNail()
    : TiebaCardModel(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    private new const string PortraitPath = "res://TiebaDIY/images/cards/EightInchNail.png";

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
        if (!ReferenceEquals(card.Owner, Owner) || Pile == null || Pile.Type == PileType.Hand)
            return;

        await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Top);
    }
}
