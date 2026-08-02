using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Baguette : ModRelicTemplate
{
    private new const string IconPath = "res://TiebaDIY/images/relics/Baguette.png";
    private const string EnergyLossVar = "EnergyLoss";

    public override RelicRarity Rarity => RelicRarity.Shop;

    public override RelicAssetProfile AssetProfile { get; } = new(
        IconPath,
        IconPath,
        IconPath);

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
