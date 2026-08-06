using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Cards;

[RegisterCard(typeof(ColorlessCardPool))]
public sealed class ElectronicSheep()
    : TiebaCardModel(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    private new const string PortraitPath = "res://TiebaDIY/images/cards/ElectronicSheep.png";

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
    ];

    public override CardAssetProfile AssetProfile { get; } = new(
        PortraitPath: PortraitPath);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var skippedCards = Owner.RunState.MapPointHistory
            .SelectMany(static act => act)
            .SelectMany(static mapPoint => mapPoint.PlayerStats)
            .Where(playerHistory => playerHistory.PlayerId == Owner.NetId)
            .SelectMany(static playerHistory => playerHistory.CardChoices)
            .Where(static choice => !choice.wasPicked)
            .Select(static choice => choice.Card)
            .Where(IsEligibleForCombatGeneration)
            .Distinct()
            .TakeRandom(3, Owner.RunState.Rng.CombatCardGeneration)
            .ToList();

        if (skippedCards.Count == 0 || CombatState is null)
            return;

        var options = new List<CardModel>(skippedCards.Count);
        foreach (var serializedCard in skippedCards)
        {
            var option = CardModel.FromSerializable(serializedCard);
            CombatState.AddCard(option, Owner);
            options.Add(option);
        }

        var selected = await CardSelectCmd.FromChooseACardScreen(
            choiceContext,
            options,
            Owner);
        if (selected is null)
            return;

        selected.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }

    private bool IsEligibleForCombatGeneration(SerializableCard serializedCard)
    {
        if (serializedCard.Id is not { } cardId)
            return false;

        var canonicalCard = ModelDb.GetByIdOrNull<CardModel>(cardId);
        if (canonicalCard is null ||
            !canonicalCard.CanBeGeneratedInCombat ||
            canonicalCard.Rarity is CardRarity.Basic or CardRarity.Ancient or CardRarity.Event)
        {
            return false;
        }

        return canonicalCard.MultiplayerConstraint switch
        {
            CardMultiplayerConstraint.MultiplayerOnly =>
                Owner.RunState.Players.Count > 1,
            CardMultiplayerConstraint.SingleplayerOnly =>
                Owner.RunState.Players.Count == 1,
            _ => true,
        };
    }
}
