using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using TiebaDIY.Scripts.Powers;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class WildHunt : TiebaRelicModel
{
    private readonly HashSet<ulong> _revivedPlayerIds = [];

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Players.Count > 1;
    }

    public override Task BeforeCombatStart()
    {
        ResetCombatState();
        return Task.CompletedTask;
    }

    public override bool ShouldDie(Creature creature)
    {
        if (creature.Player is not { } player
            || player == Owner
            || !CombatManager.Instance.IsInProgress)
        {
            return true;
        }

        return _revivedPlayerIds.Contains(player.NetId);
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature.Player is not { } player
            || player == Owner
            || !_revivedPlayerIds.Add(player.NetId))
        {
            return;
        }

        var delayPower = (WildHuntDelayPower)ModelDb.Power<WildHuntDelayPower>().ToMutable();
        delayPower.ScheduledTurn = (player.PlayerCombatState?.TurnNumber ?? 0) + 1;

        Status = RelicStatus.Active;
        Flash();
        await CreatureCmd.Heal(creature, creature.MaxHp);
        await PowerCmd.Apply(
            new ThrowingPlayerChoiceContext(),
            delayPower,
            creature,
            1,
            Owner.Creature,
            null);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        ResetCombatState();
        return Task.CompletedTask;
    }

    private void ResetCombatState()
    {
        _revivedPlayerIds.Clear();
        Status = RelicStatus.Normal;
    }
}
