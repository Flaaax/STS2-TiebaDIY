using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class NineYinManual : TiebaRelicModel
{
	private const string CursesVar = "Curses";
	private const int CardsToRemove = 6;
	private const int CursesToAdd = 3;
	private const int SpecialCurseChancePercent = 33;
	private const string FbeCurseEntryPrefix = "FBE-";
	private const string TiebaDiyCurseEntryPrefix = "TIEBA_DIY_CARD_";

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new CardsVar(CardsToRemove),
		new IntVar(CursesVar, CursesToAdd),
	];

	public override async Task AfterObtained()
	{
		var removableCardCount = PileType.Deck.GetPile(Owner).Cards.Count(static card => card.IsRemovable);
		var selectionCount = Math.Min(DynamicVars.Cards.IntValue, removableCardCount);

		if (selectionCount > 0)
		{
			var prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, selectionCount);
			var selectedCards = (await CardSelectCmd.FromDeckForRemoval(Owner, prefs)).ToList();
			if (selectedCards.Count > 0)
				await CardPileCmd.RemoveFromDeck(selectedCards);
		}

		var curses = SelectCurses();
		if (curses.Count > 0)
			await CardPileCmd.AddCursesToDeck(curses, Owner);
	}

	public override async Task AfterCardDrawn(
		PlayerChoiceContext choiceContext,
		CardModel card,
		bool fromHandDraw)
	{
		if (!ReferenceEquals(card.Owner, Owner) || card.Type != CardType.Curse)
			return;

		Flash();
		await PowerCmd.Apply<StrengthPower>(
			choiceContext,
			Owner.Creature,
			1m,
			Owner.Creature,
			null);
		await CardPileCmd.Draw(choiceContext, Owner);
	}

	private List<CardModel> SelectCurses()
	{
		var availableCurses = ModelDb.CardPool<CurseCardPool>()
			.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
			.Where(static card => card.CanBeGeneratedByModifiers)
			.OrderBy(static card => card.Id)
			.ToList();

		if (availableCurses.Count == 0)
			return [];

		List<CardModel> remainingCurses = [.. availableCurses];
		var remainingSpecialCurses = remainingCurses
			.Where(IsFbeOrTiebaDiyCurse)
			.ToList();
		List<CardModel> selectedCurses = [];
		for (var i = 0; i < DynamicVars[CursesVar].IntValue; i++)
		{
			if (remainingCurses.Count == 0)
			{
				remainingCurses.AddRange(availableCurses);
				remainingSpecialCurses.AddRange(availableCurses.Where(IsFbeOrTiebaDiyCurse));
			}

			var useSpecialPool = Owner.RunState.Rng.Niche.NextInt(100) < SpecialCurseChancePercent;
			var candidates = useSpecialPool && remainingSpecialCurses.Count > 0
				? remainingSpecialCurses
				: remainingCurses;
			var curse = Owner.RunState.Rng.Niche.NextItem(candidates);
			if (curse is null)
				break;

			selectedCurses.Add(curse);
			remainingCurses.Remove(curse);
			remainingSpecialCurses.Remove(curse);
		}

		return selectedCurses;
	}

	private static bool IsFbeOrTiebaDiyCurse(CardModel card)
	{
		var entry = card.Id.Entry;
		return entry.StartsWith(FbeCurseEntryPrefix, StringComparison.Ordinal) ||
		       entry.StartsWith(TiebaDiyCurseEntryPrefix, StringComparison.Ordinal);
	}
}
