using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class SierpinskiSponge : TiebaRelicModel
{
    private const string CombatsVar = "Combats";
    private const int MeltInterval = 4;

    private bool _isActivating;
    private int _combatsSeen;

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool ShowCounter => true;

    public override int DisplayAmount => IsActivating
        ? DynamicVars[CombatsVar].IntValue
        : CombatsSeen % DynamicVars[CombatsVar].IntValue;

    private bool IsActivating
    {
        get => _isActivating;
        set
        {
            AssertMutable();
            _isActivating = value;
            InvokeDisplayAmountChanged();
        }
    }

    [SavedProperty]
    public int CombatsSeen
    {
        get => _combatsSeen;
        private set
        {
            AssertMutable();
            _combatsSeen = value;
            InvokeDisplayAmountChanged();
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar(CombatsVar, MeltInterval),
    ];

    public override async Task AfterCombatEnd(CombatRoom _)
    {
        CombatsSeen++;
        if (CombatsSeen % DynamicVars[CombatsVar].IntValue != 0)
            return;

        await DoActivateVisuals();
        var waxRelic = Owner.Relics.FirstOrDefault(static relic => relic is { IsWax: true, IsMelted: false });
        if (waxRelic == null)
            return;

        await RelicCmd.Melt(waxRelic);
        await Cmd.CustomScaledWait(0.5f, 0.75f);
    }

    private async Task DoActivateVisuals()
    {
        IsActivating = true;
        Flash();
        await Cmd.Wait(1f);
        IsActivating = false;
    }
}
