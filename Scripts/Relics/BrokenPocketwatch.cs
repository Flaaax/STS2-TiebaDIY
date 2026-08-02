using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class BrokenPocketwatch : TiebaRelicModel
{
    private const string CardThresholdVar = "CardThreshold";

    private int _cardsPlayedThisTurn;
    private int _cardsPlayedLastTurn;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool ShowCounter => CombatManager.Instance.IsInProgress;

    public override int DisplayAmount => _cardsPlayedThisTurn;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(CardThresholdVar, 1),
        new CardsVar(5),
    ];

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || !CombatManager.Instance.IsInProgress)
            return Task.CompletedTask;

        _cardsPlayedThisTurn++;
        RefreshCounter();
        return Task.CompletedTask;
    }

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner || Owner.PlayerCombatState!.TurnNumber == 1)
            return count;

        if (_cardsPlayedLastTurn > DynamicVars[CardThresholdVar].BaseValue)
            return count;

        return count + DynamicVars.Cards.BaseValue;
    }

    public override Task AfterModifyingHandDraw()
    {
        Flash();
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature))
            return Task.CompletedTask;

        _cardsPlayedLastTurn = _cardsPlayedThisTurn;
        _cardsPlayedThisTurn = 0;
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature))
            return Task.CompletedTask;

        RefreshCounter();
        return Task.CompletedTask;
    }

    private void RefreshCounter()
    {
        Status = _cardsPlayedThisTurn <= DynamicVars[CardThresholdVar].BaseValue
            ? RelicStatus.Active
            : RelicStatus.Normal;
        InvokeDisplayAmountChanged();
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _cardsPlayedThisTurn = 0;
        _cardsPlayedLastTurn = 0;
        Status = RelicStatus.Normal;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }
}
