using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class PaelsGeneBank : ModRelicTemplate, ITiebaModel
{
    private const string RelicIconPath = "res://TiebaDIY/images/relics/PaelsGeneBank.png";

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile { get; } = new(
        IconPath: RelicIconPath,
        IconOutlinePath: RelicIconPath,
        BigIconPath: RelicIconPath);

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        HoverTipFactory.FromEnchantment<Clone>();

    public override bool TryModifyRestSiteOptions(
        Player player,
        ICollection<RestSiteOption> options)
    {
        if (player != Owner
            || player.Relics.Any(static relic => relic is PaelsGrowth)
            || !player.Deck.Cards.Any(static card => card.Enchantment is Clone))
            return false;

        options.Add(new CloneRestSiteOption(player));
        return true;
    }

    public override bool TryModifyCardRewardOptionsLate(
        Player player,
        List<CardCreationResult> cardRewards,
        CardCreationOptions options)
    {
        if (player != Owner)
            return false;

        var cloneEnchantment = ModelDb.Enchantment<Clone>();
        var modifiedAnyReward = false;

        foreach (var cardReward in cardRewards)
        {
            if (!cloneEnchantment.CanEnchant(cardReward.Card))
                continue;

            var enchantedCard = Owner.RunState.CloneCard(cardReward.Card);
            CardCmd.Enchant<Clone>(enchantedCard, 1m);
            cardReward.ModifyCard(enchantedCard, this);
            modifiedAnyReward = true;
        }

        return modifiedAnyReward;
    }
}
