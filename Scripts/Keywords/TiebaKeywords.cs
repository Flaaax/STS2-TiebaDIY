using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace TiebaDIY.Scripts.Keywords;

[RegisterOwnedCardKeyword(
    nameof(Afterlife),
    CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.None,
    IncludeInCardHoverTip = true)]
public sealed class TiebaKeywords
{
    public static readonly CardKeyword Afterlife = ModContentRegistry
        .GetQualifiedKeywordId(Entry.ModId, nameof(Afterlife))
        .GetModCardKeyword();
}
