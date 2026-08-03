using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class MechanicalArmBase : TiebaRelicModel
{
    private const string TurnsVar = "Turns";
    private const int TurnsPerActivation = 2;

    private int _turnsSinceActivation;

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool ShowCounter => true;

    public override int DisplayAmount => Math.Max(0, TurnsPerActivation - TurnsSinceActivation);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar(TurnsVar, TurnsPerActivation),
    ];

    [SavedProperty]
    private int TurnsSinceActivation
    {
        get => _turnsSinceActivation;
        set
        {
            AssertMutable();
            _turnsSinceActivation = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (player != Owner)
            return;

        TurnsSinceActivation++;
        if (TurnsSinceActivation < TurnsPerActivation)
            return;

        TurnsSinceActivation = 0;

        var card = CardFactory.GetDistinctForCombat(
                Owner,
                Owner.Character.CardPool
                    .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
                    .Where(static candidate =>
                        candidate.Type == CardType.Power && candidate.MaxUpgradeLevel > 0),
                1,
                Owner.RunState.Rng.CombatCardGeneration)
            .FirstOrDefault();

        if (card is null)
            return;

        CardCmd.Upgrade(card, CardPreviewStyle.None);
        card.EnergyCost.AddThisCombat(-1);

        Flash();
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
    }
}
