using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Enchantments;

[RegisterEnchantment]
public sealed class BaseVehicle : ModEnchantmentTemplate, ITiebaModel
{
    private const string EnchantmentIconPath =
        "res://TiebaDIY/images/enchantments/BaseVehicle.png";

    public override EnchantmentAssetProfile AssetProfile { get; } = new(
        IconPath: EnchantmentIconPath);

    public override bool ShouldStartAtBottomOfDrawPile => true;

    public override bool ShowAmount => false;

    public override async Task AfterAutoPrePlayPhaseEntered(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player != Card.Owner || player.PlayerCombatState.TurnNumber > 1)
            return;

        Card.ExhaustOnNextPlay = true;
        await CardCmd.AutoPlay(choiceContext, Card, null);
    }
}
