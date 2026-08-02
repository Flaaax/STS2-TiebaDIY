using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Baguette : TiebaRelicModel
{
    private const string EnergyLossVar = "EnergyLoss";

    public override RelicRarity Rarity => RelicRarity.Shop;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new EnergyVar(EnergyLossVar, 4),
    ];

    protected override bool IncludeEnergyHoverTip => true;

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner)
            return;

        Flash();
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);

        if (Owner.PlayerCombatState!.TurnNumber == 3)
            await PlayerCmd.LoseEnergy(DynamicVars[EnergyLossVar].BaseValue, Owner);
    }
}
