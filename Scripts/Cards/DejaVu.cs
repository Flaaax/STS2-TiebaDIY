using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using STS2RitsuLib.Audio;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Cards;

[RegisterCard(typeof(CurseCardPool))]
public sealed class DejaVu()
    : TiebaCardModel(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    private const string ChanceVar = "Chance";
    private new const string PortraitPath = "res://TiebaDIY/images/cards/DejaVu.png";
    private static readonly string[] CancelSfxPaths =
    [
        "res://TiebaDIY/audio/tick_dejavu_01.wav",
        "res://TiebaDIY/audio/tick_dejavu_02.wav",
        "res://TiebaDIY/audio/tick_dejavu_03.wav",
        "res://TiebaDIY/audio/tick_dejavu_04.wav",
        "res://TiebaDIY/audio/tick_dejavu_05.wav",
    ];

    private CardModel? _invalidatedCard;

    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable,
    ];

    public override CardAssetProfile AssetProfile { get; } = new(
        PortraitPath: PortraitPath);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(ChanceVar, 10m),
    ];

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (!ReferenceEquals(_invalidatedCard, card))
            return playCount;

        AssertMutable();

        TalkCmd.Play(
            new LocString("cards", $"{Id.Entry}.cancel"),
            Owner.Creature,
            VfxColor.Black);
        GameAudioService.Shared.PlayOneShot(
            AudioSource.ResourceFile(CancelSfxPaths[Random.Shared.Next(CancelSfxPaths.Length)]),
            new AudioPlaybackOptions { Scope = AudioLifecycleScope.Combat });

        _invalidatedCard = null;
        return 0;
    }

#if STS2_Beta
    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        CardLocation location)
    {
        if (!TryInvalidate(card))
            return location;

        location.player = card.Owner;
        location.pileType = PileType.Discard;
        location.position = CardPilePosition.Bottom;
        return location;
    }
#elif STS2_Stable
    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        PileType pileType,
        CardPilePosition position)
    {
        return TryInvalidate(card)
            ? (PileType.Discard, CardPilePosition.Bottom)
            : (pileType, position);
    }
#endif

    private bool TryInvalidate(CardModel card)
    {
        if (Pile?.Type != PileType.Hand ||
            !ReferenceEquals(card.Owner, Owner) ||
            ReferenceEquals(card, this))
        {
            return false;
        }

        if (Owner.RunState.Rng.CombatCardSelection.NextInt(100) >= DynamicVars[ChanceVar].IntValue)
            return false;

        AssertMutable();
        _invalidatedCard = card;
        return true;
    }
}
