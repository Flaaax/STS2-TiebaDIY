using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using BaseVehicleEnchantment = TiebaDIY.Scripts.Enchantments.BaseVehicle;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class BaseVehicle : ModRelicTemplate, ITiebaModel
{
    private const string RelicIconPath =
        "res://TiebaDIY/images/relics/BaseVehicle.png";

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile { get; } = new(
        IconPath: RelicIconPath,
        IconOutlinePath: RelicIconPath,
        BigIconPath: RelicIconPath);

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        HoverTipFactory.FromEnchantment<BaseVehicleEnchantment>();

    public override bool TryModifyCardRewardOptionsLate(
        Player player,
        List<CardCreationResult> cardRewards,
        CardCreationOptions options)
    {
        if (player != Owner)
            return false;

        var baseVehicleEnchantment = ModelDb.Enchantment<BaseVehicleEnchantment>();
        var validRewards = cardRewards
            .Where(reward => baseVehicleEnchantment.CanEnchant(reward.Card))
            .ToList();

        if (validRewards.Count == 0)
            return false;

        var enchantmentCount = Owner.RunState.Rng.Niche.NextInt(3) == 0 ? 2 : 1;
        enchantmentCount = Math.Min(enchantmentCount, validRewards.Count);

        for (var i = 0; i < enchantmentCount; i++)
        {
            var selectedReward = Owner.RunState.Rng.Niche.NextItem(validRewards);
            if (selectedReward == null)
                break;

            var enchantedCard = Owner.RunState.CloneCard(selectedReward.Card);
            CardCmd.Enchant<BaseVehicleEnchantment>(enchantedCard, 1m);
            selectedReward.ModifyCard(enchantedCard, this);
            validRewards.Remove(selectedReward);
        }

        return true;
    }
}
