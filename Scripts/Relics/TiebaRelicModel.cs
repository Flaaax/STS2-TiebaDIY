using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Relics;

public abstract class TiebaRelicModel : ModRelicTemplate, ITiebaModel
{
    protected virtual string RelicIconPath => $"res://TiebaDIY/images/relics/{GetType().Name}.png";

    public override RelicAssetProfile AssetProfile => new(
        IconPath: RelicIconPath,
        IconOutlinePath: RelicIconPath,
        BigIconPath: RelicIconPath);
}
