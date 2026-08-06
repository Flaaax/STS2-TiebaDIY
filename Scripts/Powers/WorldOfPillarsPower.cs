using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace TiebaDIY.Scripts.Powers;

[RegisterPower]
public sealed class WorldOfPillarsPower : TiebaPowerModel
{
    private const float RegentCardChance = 0.9f;
    private bool _isResolving;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    private bool IsResolving
    {
        get => _isResolving;
        set
        {
            AssertMutable();
            _isResolving = value;
        }
    }

    public override async Task AfterBlockGained(
        Creature creature,
        decimal amount,
        ValueProp props,
        CardModel? cardSource)
    {
        if (amount <= 0m || creature != Owner || IsResolving)
            return;

        IsResolving = true;
        try
        {
            Flash();

            var player = Owner.Player;
            var rng = player!.RunState.Rng.CombatCardGeneration;
            var generatedCards = new List<CardModel>();

            for (var i = 0; i < Amount; i++)
            {
                CardPoolModel pool = rng.NextFloat() < RegentCardChance
                    ? ModelDb.CardPool<RegentCardPool>()
                    : ModelDb.CardPool<ColorlessCardPool>();
                var candidates = pool.GetUnlockedCards(
                    player.UnlockState,
                    player.RunState.CardMultiplayerConstraint);
                var generatedCard = CardFactory.GetDistinctForCombat(player, candidates, 1, rng)
                    .FirstOrDefault();

                if (generatedCard != null)
                    generatedCards.Add(generatedCard);
            }

            if (generatedCards.Count > 0)
            {
                await CardPileCmd.AddGeneratedCardsToCombat(
                    generatedCards,
                    PileType.Hand,
                    player);
            }
        }
        finally
        {
            IsResolving = false;
        }
    }
}
