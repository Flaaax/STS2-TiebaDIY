using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Powers;

[RegisterPower]
public sealed class WildHuntDelayPower : ModPowerTemplate, ITiebaModel
{
    private const int DoomAmount = 999;
    private int _scheduledTurn;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    protected override bool IsVisibleInternal => false;

    public override bool ShouldPlayVfx => false;

    [SavedProperty]
    public int ScheduledTurn
    {
        get => _scheduledTurn;
        set
        {
            AssertMutable();
            _scheduledTurn = value;
        }
    }

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player || (player.PlayerCombatState?.TurnNumber ?? 0) < ScheduledTurn)
            return;

        await PowerCmd.Apply<DoomPower>(
            new ThrowingPlayerChoiceContext(),
            Owner,
            DoomAmount,
            Applier,
            null);
    }

    public override async Task AfterSideTurnEndLate(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player
            || !participants.Contains(Owner)
            || (Owner.Player?.PlayerCombatState?.TurnNumber ?? 0) < ScheduledTurn)
        {
            return;
        }

        await PowerCmd.Remove(this);
    }

    public override async Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
    {
        if (!Owner.IsDead)
            await CreatureCmd.SetCurrentHp(Owner, 1m);
    }
}
