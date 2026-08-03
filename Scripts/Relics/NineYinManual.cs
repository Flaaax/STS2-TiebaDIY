using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace TiebaDIY.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class NineYinManual : TiebaRelicModel
{
	private const string CursesVar = "Curses";
	private const int CardsToRemove = 6;
	private const int CursesToAdd = 3;

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

		var availableCurses = ModelDb.CardPool<CurseCardPool>()
			.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
			.Where(static card => card.CanBeGeneratedByModifiers)
			.OrderBy(static card => card.Id)
			.ToList();

		if (availableCurses.Count == 0)
			return;

		List<CardModel> remainingCurses = [.. availableCurses];
		List<CardModel> curses = [];
		for (var i = 0; i < DynamicVars[CursesVar].IntValue; i++)
		{
			if (remainingCurses.Count == 0)
				remainingCurses.AddRange(availableCurses);

			var curse = Owner.RunState.Rng.Niche.NextItem(remainingCurses);
			if (curse is null)
			{
				return;
			}

			curses.Add(curse);
			remainingCurses.Remove(curse);
		}

		await CardPileCmd.AddCursesToDeck(curses, Owner);
	}
}