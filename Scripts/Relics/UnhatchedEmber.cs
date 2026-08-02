using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class UnhatchedEmber : TiebaRelicModel
{
    private bool _wasUsedThisCombat;
    private bool _isProtectingThisTurn;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override string RelicIconPath => "res://TiebaDIY/images/relics/UnhatchedEmber.webp";

    public override Task BeforeCombatStart()
    {
        _wasUsedThisCombat = false;
        _isProtectingThisTurn = false;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }

    public override bool ShouldDie(Creature creature)
    {
        if (creature != Owner.Creature || !CombatManager.Instance.IsInProgress)
            return true;

        return _wasUsedThisCombat && !_isProtectingThisTurn;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature != Owner.Creature)
            return;

        if (!_wasUsedThisCombat)
        {
            _wasUsedThisCombat = true;
            _isProtectingThisTurn = true;
            Status = RelicStatus.Active;
            Flash();
        }

        await CreatureCmd.Heal(creature, 1);
    }

    public override decimal ModifyHpLostAfterOstyLate(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner.Creature || !_isProtectingThisTurn)
            return amount;

        return Math.Min(amount, Math.Max(target.CurrentHp - 1, 0));
    }

    public override Task AfterSideTurnEndLate(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!_isProtectingThisTurn)
            return Task.CompletedTask;

        _isProtectingThisTurn = false;
        Status = RelicStatus.Disabled;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _wasUsedThisCombat = false;
        _isProtectingThisTurn = false;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }
}
