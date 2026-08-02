using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TiebaDIY.Scripts.Ancients;

[RegisterSharedAncient]
public sealed class GodOfDIY : ModAncientEventTemplate, ITiebaModel
{
	private const string PresentationIconPath = "res://TiebaDIY/images/ancients/GodOfDIYIcon.svg";

	public override Color ButtonColor => new(0.25f, 0.25f, 0.25f, 0.75f);

	public override Color DialogueColor => new(0.1f, 0.1f, 0.1f);

	public override EventAssetProfile AssetProfile => new(
		BackgroundScenePath: "res://TiebaDIY/scenes/ancients/GodOfDIY.tscn"
	);

	public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile => new(
		MapIconPath: PresentationIconPath,
		MapIconOutlinePath: PresentationIconPath,
		RunHistoryIconPath: PresentationIconPath,
		RunHistoryIconOutlinePath: PresentationIconPath
	);

	private IReadOnlyList<EventOption> Options =>
	[
		CreateModRelicOption<Anchor>(),
		CreateModRelicOption<BagOfPreparation>(),
		CreateModRelicOption<Lantern>()
	];

	public override IEnumerable<EventOption> AllPossibleOptions => Options;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions() => Options;
}
