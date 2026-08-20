using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using TiebaDIY.Scripts.Cards;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class PremiumBigFruit : TiebaRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        .. HoverTipFactory.FromCardWithCardHoverTips<OralFibrosis>(),
        .. HoverTipFactory.FromEnchantment<Sown>(),
    ];

    public override async Task AfterObtained()
    {
        await CardPileCmd.AddCursesToDeck(
            [ModelDb.Card<OralFibrosis>()],
            Owner);
    }

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player != Owner)
            return Task.CompletedTask;

        var sown = ModelDb.Enchantment<Sown>();
        var enchantableCards = PileType.Hand.GetPile(Owner).Cards
            .Where(sown.CanEnchant)
            .ToList();

        if (enchantableCards.Count == 0)
            return Task.CompletedTask;

        var enchantmentCount = Math.Min(
            Owner.RunState.Rng.Niche.NextInt(2) + 1,
            enchantableCards.Count);

        for (var i = 0; i < enchantmentCount; i++)
        {
            var card = Owner.RunState.Rng.Niche.NextItem(enchantableCards);
            if (card is null)
                break;

            CardCmd.Enchant<Sown>(card, 1m);
            enchantableCards.Remove(card);
        }

        Flash();
        return Task.CompletedTask;
    }
}
