using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class PaelsGeneBank : ModRelicTemplate, ITiebaModel
{
    private const string RelicIconPath = "res://TiebaDIY/images/relics/PaelsGeneBank.png";
    private const int RewardOptionCount = 3;
    private const float NonBasicNonCurseChance = 0.9f;

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile { get; } = new(
        IconPath: RelicIconPath,
        IconOutlinePath: RelicIconPath,
        BigIconPath: RelicIconPath);

    public override bool TryModifyRewards(
        Player player,
        List<Reward> rewards,
        AbstractRoom? room)
    {
        if (player != Owner || room is not CombatRoom)
            return false;

        var nonBasicNonCurses = new List<CardModel>();
        var basicsOrCurses = new List<CardModel>();

        foreach (var card in player.Deck.Cards)
        {
            if (card.Type == CardType.Quest)
                continue;

            if (card.Rarity == CardRarity.Basic || card.Type == CardType.Curse)
                basicsOrCurses.Add(card);
            else
                nonBasicNonCurses.Add(card);
        }

        var rewardCards = SelectRewardCards(player, nonBasicNonCurses, basicsOrCurses);
        if (rewardCards.Count == 0)
            return false;

        rewards.Add(new CardReward(
            rewardCards,
            CardCreationSource.Encounter,
            player,
            CardCreationOptions.ForRoom(player, room.RoomType)));
        return true;
    }

    private static List<CardModel> SelectRewardCards(
        Player player,
        List<CardModel> nonBasicNonCurses,
        List<CardModel> basicsOrCurses)
    {
        var selectedCards = new List<CardModel>(RewardOptionCount);
        var rng = player.PlayerRng.Rewards;

        while (selectedCards.Count < RewardOptionCount
               && (nonBasicNonCurses.Count > 0 || basicsOrCurses.Count > 0))
        {
            var preferredCards = rng.NextFloat() < NonBasicNonCurseChance
                ? nonBasicNonCurses
                : basicsOrCurses;
            var fallbackCards = preferredCards == nonBasicNonCurses
                ? basicsOrCurses
                : nonBasicNonCurses;
            var sourceCards = preferredCards.Count > 0 ? preferredCards : fallbackCards;

            var selectedIndex = rng.NextInt(sourceCards.Count);
            selectedCards.Add(player.RunState.CloneCard(sourceCards[selectedIndex]));
            sourceCards.RemoveAt(selectedIndex);
        }

        return selectedCards;
    }
}
