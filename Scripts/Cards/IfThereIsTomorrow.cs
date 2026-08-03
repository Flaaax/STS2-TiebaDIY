using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using TiebaDIY.Scripts.Keywords;

namespace TiebaDIY.Scripts.Cards;

[RegisterCard(typeof(NecrobinderCardPool))]
public sealed class IfThereIsTomorrow()
    : TiebaCardModel(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    private new const string PortraitPath = "res://TiebaDIY/images/cards/IfThereIsTomorrow.png";

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Ethereal,
        TiebaKeywords.Afterlife,
    ];

    public override CardAssetProfile AssetProfile { get; } = new(
        PortraitPath: PortraitPath);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<DoomPower>(24m),
        new CardsVar(3),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<DoomPower>(),
        HoverTipFactory.FromCard<Wisp>(upgrade: true),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var wisps = new List<CardModel>();
        for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            var wisp = CombatState!.CreateCard<Wisp>(Owner);
            CardCmd.Upgrade(wisp, CardPreviewStyle.None);
            wisps.Add(wisp);
        }

        await CardPileCmd.AddGeneratedCardsToCombat(wisps, PileType.Hand, Owner);
        await PowerCmd.Apply<DoomPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Doom.BaseValue,
            Owner.Creature,
            this);
    }

    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        if (card == this && Owner.IsOstyAlive)
            await CreatureCmd.Kill(Owner.Osty!);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
