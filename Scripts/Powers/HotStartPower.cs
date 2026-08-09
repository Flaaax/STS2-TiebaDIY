using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace TiebaDIY.Scripts.Powers;

[RegisterPower]
public sealed class HotStartPower : TiebaPowerModel
{
    private bool _isResolving;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Channeling),
    ];

    private bool IsResolving
    {
        get => _isResolving;
        set
        {
            AssertMutable();
            _isResolving = value;
        }
    }

    public override async Task AfterOrbChanneled(
        PlayerChoiceContext choiceContext,
        Player player,
        OrbModel orb)
    {
        if (player != Owner.Player || IsResolving)
            return;

        IsResolving = true;
        try
        {
            Flash();
            for (var i = 0; i < Amount; i++)
            {
                await OrbCmd.Passive(choiceContext, orb, null);
                // await Cmd.Wait(0.25f);
            }
        }
        finally
        {
            IsResolving = false;
        }
    }
}
