using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class TechTeaSet : TiebaRelicModel
{
    private const string ChargesGainedVar = "ChargesGained";

    private int _charges;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool ShowCounter => true;

    public override int DisplayAmount => Charges;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(ChargesGainedVar, 2),
        new EnergyVar(1),
    ];

    protected override bool IncludeEnergyHoverTip => true;

    [SavedProperty]
    public int Charges
    {
        get => _charges;
        set
        {
            AssertMutable();
            _charges = value;
            Status = _charges > 0 ? RelicStatus.Active : RelicStatus.Normal;
            InvokeDisplayAmountChanged();
        }
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not RestSiteRoom)
            return Task.CompletedTask;

        Charges += DynamicVars[ChargesGainedVar].IntValue;
        Flash();
        return Task.CompletedTask;
    }

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner || Owner.PlayerCombatState!.TurnNumber != 1 || Charges <= 0)
            return;

        Charges--;
        Flash();
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
    }
}
