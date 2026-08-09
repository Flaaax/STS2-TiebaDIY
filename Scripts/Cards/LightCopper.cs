using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Cards;

[RegisterCard(typeof(CurseCardPool))]
public sealed class LightCopper()
    : TiebaCardModel(2, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    private new const string PortraitPath = "res://TiebaDIY/images/cards/LightCopper.png";

    public override int MaxUpgradeLevel => 0;

    public override bool HasTurnEndInHandEffect => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
    ];

    public override CardAssetProfile AssetProfile { get; } = new(
        PortraitPath: PortraitPath);

    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        await Cmd.Wait(0.25f);

        var copies = new[] { CreateClone() }
            .Concat(PileType.Hand
                .GetPile(Owner)
                .Cards
                .Select(static handCard => handCard.CreateClone()))
            .ToList();

        await CardPileCmd.AddGeneratedCardsToCombat(copies, PileType.Hand, Owner);
    }
}
