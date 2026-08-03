using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class GoodStopwatch : TiebaRelicModel
{
    private const string TurnVar = "Turn";
    private const int ExtraTurnAfterTurn = 3;

    private bool _usedThisCombat;

    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(TurnVar, ExtraTurnAfterTurn),
    ];

    private bool UsedThisCombat
    {
        get => _usedThisCombat;
        set
        {
            AssertMutable();
            _usedThisCombat = value;
        }
    }

    public override bool ShouldTakeExtraTurn(Player player)
    {
        return player == Owner &&
               !UsedThisCombat &&
               Owner.PlayerCombatState?.TurnNumber == DynamicVars[TurnVar].IntValue;
    }

    public override Task AfterTakingExtraTurn(Player player)
    {
        if (!ShouldTakeExtraTurn(player))
            return Task.CompletedTask;

        UsedThisCombat = true;
        Status = RelicStatus.Normal;
        Flash();
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature))
        {
            Status = !UsedThisCombat &&
                     Owner.PlayerCombatState?.TurnNumber == DynamicVars[TurnVar].IntValue
                ? RelicStatus.Active
                : RelicStatus.Normal;
        }

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        UsedThisCombat = false;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }
}
