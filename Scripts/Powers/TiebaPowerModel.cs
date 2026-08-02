using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Powers;

public abstract class TiebaPowerModel : ModPowerTemplate, ITiebaModel
{
    protected string PowerIconPath => $"res://TiebaDIY/images/powers/{GetType().Name}.png";

    public override PowerAssetProfile AssetProfile => new(
        IconPath: PowerIconPath,
        BigIconPath: PowerIconPath);
}
