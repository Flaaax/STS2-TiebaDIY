using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class EggWaffle : TiebaRelicModel
{
    private int _storedEnergy;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool ShowCounter => StoredEnergy > 0;

    public override int DisplayAmount => StoredEnergy;

    protected override bool IncludeEnergyHoverTip => true;

    [SavedProperty]
    public int StoredEnergy
    {
        get => _storedEnergy;
        set
        {
            AssertMutable();
            _storedEnergy = Math.Max(0, value);
            Status = _storedEnergy > 0 ? RelicStatus.Active : RelicStatus.Normal;
            InvokeDisplayAmountChanged();
        }
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        StoredEnergy = Owner.PlayerCombatState?.Energy ?? 0;
        if (StoredEnergy > 0)
            Flash();

        return Task.CompletedTask;
    }

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner || Owner.PlayerCombatState!.TurnNumber != 1 || StoredEnergy <= 0)
            return;

        int energyToRestore = StoredEnergy;
        StoredEnergy = 0;
        Flash();
        await PlayerCmd.GainEnergy(energyToRestore, Owner);
    }
}
